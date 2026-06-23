using CommunityEvents.Data;
using CommunityEvents.Exceptions;
using CommunityEvents.Models;
using CommunityEvents.Repositories;
using CommunityEvents.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CommunityEvents.Tests.Services;

public class ParticipantServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ParticipantService _service;

    public ParticipantServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();
        _service = new ParticipantService(new ParticipantRepository(_context));
    }

    [Fact]
    public async Task CreateParticipantAsync_ValidParticipant_ReturnsWithId()
    {
        var p = new Participant { FirstName = "Jane", LastName = "Doe", Email = "jane@example.com" };
        var created = await _service.CreateParticipantAsync(p);
        created.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateParticipantAsync_DuplicateEmail_ThrowsDuplicateEntityException()
    {
        var p1 = new Participant { FirstName = "A", LastName = "B", Email = "dup@example.com" };
        await _service.CreateParticipantAsync(p1);

        var p2 = new Participant { FirstName = "C", LastName = "D", Email = "dup@example.com" };
        var act = () => _service.CreateParticipantAsync(p2);

        await act.Should().ThrowAsync<DuplicateEntityException>();
    }

    [Fact]
    public async Task CreateParticipantAsync_EmptyFirstName_ThrowsDomainValidationException()
    {
        var p = new Participant { FirstName = "", LastName = "Doe", Email = "ok@example.com" };
        var act = () => _service.CreateParticipantAsync(p);
        await act.Should().ThrowAsync<DomainValidationException>();
    }

    [Fact]
    public async Task CreateParticipantAsync_InvalidEmail_ThrowsDomainValidationException()
    {
        var p = new Participant { FirstName = "Test", LastName = "User", Email = "not-an-email" };
        var act = () => _service.CreateParticipantAsync(p);
        await act.Should().ThrowAsync<DomainValidationException>();
    }

    [Fact]
    public async Task GetParticipantByEmailAsync_ExistingEmail_ReturnsParticipant()
    {
        await _service.CreateParticipantAsync(new Participant { FirstName = "Bob", LastName = "Smith", Email = "bob@example.com" });
        var found = await _service.GetParticipantByEmailAsync("bob@example.com");
        found.Should().NotBeNull();
        found!.FullName.Should().Be("Bob Smith");
    }

    [Fact]
    public async Task DeleteParticipantAsync_ExistingParticipant_SoftDeletes()
    {
        var p = await _service.CreateParticipantAsync(new Participant { FirstName = "Del", LastName = "Me", Email = "del@example.com" });
        await _service.DeleteParticipantAsync(p.Id);

        var act = () => _service.GetParticipantDetailsAsync(p.Id);
        var result = await _service.GetParticipantDetailsAsync(p.Id);
        result.Should().BeNull(); // soft-deleted, so not returned
    }

    [Fact]
    public async Task DeleteParticipantAsync_NonExistentId_ThrowsParticipantNotFoundException()
    {
        var act = () => _service.DeleteParticipantAsync(9999);
        await act.Should().ThrowAsync<ParticipantNotFoundException>();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
