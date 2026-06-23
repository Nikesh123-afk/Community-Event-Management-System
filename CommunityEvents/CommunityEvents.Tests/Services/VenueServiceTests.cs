using CommunityEvents.Data;
using CommunityEvents.Exceptions;
using CommunityEvents.Models;
using CommunityEvents.Repositories;
using CommunityEvents.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CommunityEvents.Tests.Services;

public class VenueServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly VenueService _service;

    public VenueServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();
        _service = new VenueService(new VenueRepository(_context));
    }

    [Fact]
    public async Task CreateVenueAsync_ValidVenue_ReturnsWithId()
    {
        var venue = new Venue { Name = "Test Hall", Address = "1 Test Rd", Capacity = 100 };
        var created = await _service.CreateVenueAsync(venue);
        created.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateVenueAsync_ZeroCapacity_ThrowsDomainValidationException()
    {
        var venue = new Venue { Name = "Bad Venue", Address = "1 Rd", Capacity = 0 };
        var act = () => _service.CreateVenueAsync(venue);
        await act.Should().ThrowAsync<DomainValidationException>();
    }

    [Fact]
    public async Task CreateVenueAsync_EmptyName_ThrowsDomainValidationException()
    {
        var venue = new Venue { Name = "", Address = "1 Rd", Capacity = 50 };
        var act = () => _service.CreateVenueAsync(venue);
        await act.Should().ThrowAsync<DomainValidationException>();
    }

    [Fact]
    public async Task GetAllVenuesAsync_ReturnsOnlyNonDeletedVenues()
    {
        await _service.CreateVenueAsync(new Venue { Name = "Venue A", Address = "1 A St", Capacity = 50 });
        var toDelete = await _service.CreateVenueAsync(new Venue { Name = "Venue B", Address = "2 B St", Capacity = 30 });
        await _service.DeleteVenueAsync(toDelete.Id);

        var result = await _service.GetAllVenuesAsync();
        result.Should().Contain(v => v.Name == "Venue A");
        result.Should().NotContain(v => v.Name == "Venue B");
    }

    [Fact]
    public async Task UpdateVenueAsync_NonExistentId_ThrowsVenueNotFoundException()
    {
        var venue = new Venue { Id = 9999, Name = "Ghost", Address = "Nowhere", Capacity = 10 };
        var act = () => _service.UpdateVenueAsync(venue);
        await act.Should().ThrowAsync<VenueNotFoundException>();
    }

    [Fact]
    public async Task CanAccommodate_EnoughCapacity_ReturnsTrue()
    {
        var venue = new Venue { Name = "Big Hall", Address = "10 Big St", Capacity = 500 };
        venue.CanAccommodate(400).Should().BeTrue();
    }

    [Fact]
    public async Task CanAccommodate_ExceedsCapacity_ReturnsFalse()
    {
        var venue = new Venue { Name = "Small Room", Address = "5 Sm Ln", Capacity = 10 };
        venue.CanAccommodate(15).Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
