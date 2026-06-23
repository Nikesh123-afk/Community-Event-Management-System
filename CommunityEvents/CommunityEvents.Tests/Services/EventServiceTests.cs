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

/// <summary>
/// Unit tests for EventService using the EF Core In-Memory provider.
/// Demonstrates unit testing with xUnit, Moq, FluentAssertions, and the
/// Arrange-Act-Assert (AAA) pattern.
/// </summary>
public class EventServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly EventRepository _eventRepo;
    private readonly Mock<INotificationService> _notificationMock;
    private readonly EventService _service;

    public EventServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();

        _eventRepo = new EventRepository(_context);
        _notificationMock = new Mock<INotificationService>();
        _service = new EventService(_eventRepo, _context, _notificationMock.Object);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var venue = new Venue { Name = "Test Hall", Address = "123 Test St", Capacity = 100 };
        _context.Venues.Add(venue);
        _context.SaveChanges();

        var futureEvent = new Event
        {
            Name = "Future Tech Summit",
            Description = "A detailed description of the future tech summit event happening soon.",
            EventDate = DateTime.UtcNow.AddDays(30),
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            VenueId = venue.Id,
            MaxCapacity = 50,
            Status = EventStatus.Published,
            Type = EventType.InPerson
        };

        var pastEvent = new Event
        {
            Name = "Past Community Day",
            Description = "An event that happened in the past and should not accept registrations.",
            EventDate = DateTime.UtcNow.AddDays(-10),
            StartTime = new TimeSpan(10, 0, 0),
            EndTime = new TimeSpan(16, 0, 0),
            VenueId = venue.Id,
            MaxCapacity = 30,
            Status = EventStatus.Completed,
            Type = EventType.InPerson
        };

        _context.Events.AddRange(futureEvent, pastEvent);
        _context.SaveChanges();
    }

    // ── GetAllEvents ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllEventsAsync_ReturnsAllNonDeletedEvents()
    {
        // Arrange — data seeded in constructor

        // Act
        var result = await _service.GetAllEventsAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    // ── GetUpcomingEvents ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetUpcomingEventsAsync_ReturnsOnlyFuturePublishedEvents()
    {
        // Act
        var result = await _service.GetUpcomingEventsAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Future Tech Summit");
    }

    [Fact]
    public async Task GetUpcomingEventsAsync_DoesNotReturnPastEvents()
    {
        var result = await _service.GetUpcomingEventsAsync();
        result.Should().NotContain(e => e.Name == "Past Community Day");
    }

    // ── CreateEvent ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateEventAsync_WithValidData_PersistsAndReturnsEvent()
    {
        // Arrange
        var venue = _context.Venues.First();
        var newEvent = new Event
        {
            Name = "New Workshop",
            Description = "A comprehensive description for the new workshop event being created.",
            EventDate = DateTime.UtcNow.AddDays(14),
            StartTime = new TimeSpan(10, 0, 0),
            EndTime = new TimeSpan(12, 0, 0),
            VenueId = venue.Id,
            MaxCapacity = 20,
            Type = EventType.InPerson
        };

        // Act
        var created = await _service.CreateEventAsync(newEvent);

        // Assert
        created.Id.Should().BeGreaterThan(0);
        created.Name.Should().Be("New Workshop");

        var fromDb = await _context.Events.FindAsync(created.Id);
        fromDb.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateEventAsync_WithEmptyName_ThrowsDomainValidationException()
    {
        // Arrange
        var venue = _context.Venues.First();
        var invalid = new Event
        {
            Name = "",  // INVALID
            Description = "Some description here",
            EventDate = DateTime.UtcNow.AddDays(14),
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(12, 0, 0),
            VenueId = venue.Id,
            MaxCapacity = 10
        };

        // Act
        var act = () => _service.CreateEventAsync(invalid);

        // Assert
        await act.Should().ThrowAsync<DomainValidationException>();
    }

    [Fact]
    public async Task CreateEventAsync_WithZeroCapacity_ThrowsDomainValidationException()
    {
        var venue = _context.Venues.First();
        var act = () => _service.CreateEventAsync(new Event
        {
            Name = "Some Event",
            Description = "Some detailed description for this event",
            EventDate = DateTime.UtcNow.AddDays(14),
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(12, 0, 0),
            VenueId = venue.Id,
            MaxCapacity = 0  // INVALID
        });

        await act.Should().ThrowAsync<DomainValidationException>();
    }

    // ── PublishEvent ──────────────────────────────────────────────────────────

    [Fact]
    public async Task PublishEventAsync_DraftEvent_ChangesStatusToPublished()
    {
        // Arrange
        var venue = _context.Venues.First();
        var draft = new Event
        {
            Name = "Draft Event",
            Description = "This is a detailed description for a draft event that is about to be published.",
            EventDate = DateTime.UtcNow.AddDays(5),
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(11, 0, 0),
            VenueId = venue.Id,
            MaxCapacity = 10,
            Status = EventStatus.Draft
        };
        _context.Events.Add(draft);
        await _context.SaveChangesAsync();

        // Act
        await _service.PublishEventAsync(draft.Id);

        // Assert
        var updated = await _context.Events.FindAsync(draft.Id);
        updated!.Status.Should().Be(EventStatus.Published);
        _notificationMock.Verify(n => n.NotifyEventPublishedAsync(It.IsAny<Event>()), Times.Once);
    }

    [Fact]
    public async Task PublishEventAsync_NonExistentId_ThrowsEventNotFoundException()
    {
        var act = () => _service.PublishEventAsync(9999);
        await act.Should().ThrowAsync<EventNotFoundException>();
    }

    // ── CancelEvent ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CancelEventAsync_PublishedEvent_ChangesStatusToCancelled()
    {
        // Arrange
        var eventId = _context.Events.First(e => e.Status == EventStatus.Published).Id;

        // Act
        await _service.CancelEventAsync(eventId);

        // Assert
        var updated = await _context.Events.FindAsync(eventId);
        updated!.Status.Should().Be(EventStatus.Cancelled);
    }

    // ── DeleteEvent ───────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteEventAsync_ExistingEvent_SoftDeletesIt()
    {
        var eventId = _context.Events.First().Id;

        await _service.DeleteEventAsync(eventId);

        var deleted = await _context.Events.FindAsync(eventId);
        deleted!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteEventAsync_NonExistentId_ThrowsEventNotFoundException()
    {
        var act = () => _service.DeleteEventAsync(9999);
        await act.Should().ThrowAsync<EventNotFoundException>();
    }

    // ── FilterEvents ─────────────────────────────────────────────────────────

    [Fact]
    public async Task FilterEventsAsync_ByKeyword_ReturnsMatchingEvents()
    {
        // Act
        var result = await _service.FilterEventsAsync(null, null, null, "tech");

        // Assert — only "Future Tech Summit" contains "tech"
        result.Should().HaveCount(1);
        result.First().Name.Should().Contain("Tech");
    }

    [Fact]
    public async Task FilterEventsAsync_NoFilters_ReturnsAllPublishedEvents()
    {
        var result = await _service.FilterEventsAsync(null, null, null, null);
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(e => e.Status == EventStatus.Published);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
