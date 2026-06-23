using CommunityEvents.Data;
using CommunityEvents.Exceptions;
using CommunityEvents.Interfaces;
using CommunityEvents.Models;
using CommunityEvents.Repositories;
using CommunityEvents.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CommunityEvents.Tests.Services;

public class RegistrationServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly RegistrationService _service;

    private Venue _venue = null!;
    private Event _openEvent = null!;
    private Event _fullEvent = null!;
    private Event _pastEvent = null!;
    private Participant _participant = null!;

    public RegistrationServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();

        var notificationMock = new Mock<INotificationService>();
        notificationMock.Setup(n => n.NotifyRegistrationConfirmedAsync(It.IsAny<Registration>()))
            .Returns(Task.CompletedTask);

        _service = new RegistrationService(
            new RegistrationRepository(_context),
            new EventRepository(_context),
            new ParticipantRepository(_context),
            notificationMock.Object);

        SeedData();
    }

    private void SeedData()
    {
        _venue = new Venue { Name = "Test Venue", Address = "1 Test Rd", Capacity = 200 };
        _context.Venues.Add(_venue);
        _context.SaveChanges();

        _participant = new Participant { FirstName = "Alice", LastName = "Smith", Email = "alice@test.com" };
        _context.Participants.Add(_participant);

        _openEvent = new Event
        {
            Name = "Open Event",
            Description = "An open event with available capacity for testing registrations.",
            EventDate = DateTime.UtcNow.AddDays(10),
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(17),
            VenueId = _venue.Id,
            MaxCapacity = 50,
            Status = EventStatus.Published
        };

        _fullEvent = new Event
        {
            Name = "Full Event",
            Description = "A fully booked event that should trigger wait-listing for new registrations.",
            EventDate = DateTime.UtcNow.AddDays(10),
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(17),
            VenueId = _venue.Id,
            MaxCapacity = 1,
            Status = EventStatus.Published
        };

        _pastEvent = new Event
        {
            Name = "Past Event",
            Description = "A past event that has already occurred and should reject new registrations.",
            EventDate = DateTime.UtcNow.AddDays(-5),
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(17),
            VenueId = _venue.Id,
            MaxCapacity = 50,
            Status = EventStatus.Completed
        };

        _context.Events.AddRange(_openEvent, _fullEvent, _pastEvent);
        _context.SaveChanges();

        // Fill up the full event
        var existingParticipant = new Participant { FirstName = "Bob", LastName = "Jones", Email = "bob@test.com" };
        _context.Participants.Add(existingParticipant);
        _context.SaveChanges();

        _context.Registrations.Add(new Registration
        {
            ParticipantId = existingParticipant.Id,
            EventId = _fullEvent.Id,
            Status = RegistrationStatus.Confirmed,
            RegistrationDate = DateTime.UtcNow
        });
        _context.SaveChanges();
    }

    [Fact]
    public async Task RegisterParticipantAsync_OpenEvent_ReturnsConfirmedRegistration()
    {
        // Act
        var reg = await _service.RegisterParticipantAsync(_participant.Id, _openEvent.Id);

        // Assert
        reg.Status.Should().Be(RegistrationStatus.Confirmed);
        reg.ParticipantId.Should().Be(_participant.Id);
        reg.EventId.Should().Be(_openEvent.Id);
    }

    [Fact]
    public async Task RegisterParticipantAsync_FullEvent_ReturnsWaitListedRegistration()
    {
        // Act
        var reg = await _service.RegisterParticipantAsync(_participant.Id, _fullEvent.Id);

        // Assert
        reg.Status.Should().Be(RegistrationStatus.WaitListed);
    }

    [Fact]
    public async Task RegisterParticipantAsync_PastEvent_ThrowsEventAlreadyPassedException()
    {
        var act = () => _service.RegisterParticipantAsync(_participant.Id, _pastEvent.Id);
        await act.Should().ThrowAsync<EventAlreadyPassedException>();
    }

    [Fact]
    public async Task RegisterParticipantAsync_DuplicateRegistration_ThrowsAlreadyRegisteredException()
    {
        // Arrange — register once
        await _service.RegisterParticipantAsync(_participant.Id, _openEvent.Id);

        // Act — try to register again
        var act = () => _service.RegisterParticipantAsync(_participant.Id, _openEvent.Id);

        // Assert
        await act.Should().ThrowAsync<AlreadyRegisteredException>();
    }

    [Fact]
    public async Task RegisterParticipantAsync_InvalidParticipantId_ThrowsParticipantNotFoundException()
    {
        var act = () => _service.RegisterParticipantAsync(9999, _openEvent.Id);
        await act.Should().ThrowAsync<ParticipantNotFoundException>();
    }

    [Fact]
    public async Task CancelRegistrationAsync_ExistingRegistration_SetsCancelledStatus()
    {
        // Arrange
        var reg = await _service.RegisterParticipantAsync(_participant.Id, _openEvent.Id);

        // Act
        var cancelled = await _service.CancelRegistrationAsync(reg.Id);

        // Assert
        cancelled.Status.Should().Be(RegistrationStatus.Cancelled);
    }

    [Fact]
    public async Task CancelRegistrationAsync_NonExistentId_ThrowsRegistrationNotFoundException()
    {
        var act = () => _service.CancelRegistrationAsync(9999);
        await act.Should().ThrowAsync<RegistrationNotFoundException>();
    }

    [Fact]
    public async Task IsParticipantRegisteredAsync_AfterRegistration_ReturnsTrue()
    {
        await _service.RegisterParticipantAsync(_participant.Id, _openEvent.Id);
        var result = await _service.IsParticipantRegisteredAsync(_participant.Id, _openEvent.Id);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsParticipantRegisteredAsync_BeforeRegistration_ReturnsFalse()
    {
        var result = await _service.IsParticipantRegisteredAsync(_participant.Id, _openEvent.Id);
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
