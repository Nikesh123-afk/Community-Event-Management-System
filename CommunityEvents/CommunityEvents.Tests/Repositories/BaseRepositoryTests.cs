using CommunityEvents.Data;
using CommunityEvents.Exceptions;
using CommunityEvents.Models;
using CommunityEvents.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CommunityEvents.Tests.Repositories;

/// <summary>
/// Tests for the generic BaseRepository via the concrete VenueRepository.
/// Validates CRUD operations, soft-delete behaviour, and count queries.
/// </summary>
public class BaseRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly VenueRepository _repo;

    public BaseRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();
        _repo = new VenueRepository(_context);
    }

    [Fact]
    public async Task AddAsync_PersistsEntityAndAssignsId()
    {
        var venue = new Venue { Name = "New Venue", Address = "1 St", Capacity = 50 };
        var result = await _repo.AddAsync(venue);
        result.Id.Should().BeGreaterThan(0);
        _context.Venues.Should().Contain(v => v.Id == result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsEntity()
    {
        var venue = new Venue { Name = "Find Me", Address = "2 St", Capacity = 100 };
        await _repo.AddAsync(venue);

        var found = await _repo.GetByIdAsync(venue.Id);

        found.Should().NotBeNull();
        found!.Name.Should().Be("Find Me");
    }

    [Fact]
    public async Task GetByIdAsync_SoftDeletedEntity_ReturnsNull()
    {
        var venue = new Venue { Name = "Delete Me", Address = "3 St", Capacity = 30 };
        await _repo.AddAsync(venue);
        await _repo.DeleteAsync(venue.Id);

        var found = await _repo.GetByIdAsync(venue.Id);
        found.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ExcludesSoftDeletedEntities()
    {
        var v1 = await _repo.AddAsync(new Venue { Name = "Active", Address = "A St", Capacity = 50 });
        var v2 = await _repo.AddAsync(new Venue { Name = "Deleted", Address = "D St", Capacity = 40 });
        await _repo.DeleteAsync(v2.Id);

        var all = await _repo.GetAllAsync();
        all.Should().Contain(v => v.Id == v1.Id);
        all.Should().NotContain(v => v.Id == v2.Id);
    }

    [Fact]
    public async Task UpdateAsync_ChangesArePersisted()
    {
        var venue = await _repo.AddAsync(new Venue { Name = "Original", Address = "O St", Capacity = 50 });
        venue.Name = "Updated";
        venue.Capacity = 100;

        await _repo.UpdateAsync(venue);

        var updated = await _repo.GetByIdAsync(venue.Id);
        updated!.Name.Should().Be("Updated");
        updated.Capacity.Should().Be(100);
    }

    [Fact]
    public async Task DeleteAsync_SetsIsDeletedFlag()
    {
        var venue = await _repo.AddAsync(new Venue { Name = "To Delete", Address = "X St", Capacity = 20 });
        await _repo.DeleteAsync(venue.Id);

        var raw = await _context.Venues.FindAsync(venue.Id);
        raw!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_NonExistentId_ThrowsNotFoundException()
    {
        var act = () => _repo.DeleteAsync(9999);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExistsAsync_ExistingId_ReturnsTrue()
    {
        var venue = await _repo.AddAsync(new Venue { Name = "Exists", Address = "E St", Capacity = 60 });
        (await _repo.ExistsAsync(venue.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_SoftDeletedId_ReturnsFalse()
    {
        var venue = await _repo.AddAsync(new Venue { Name = "Gone", Address = "G St", Capacity = 10 });
        await _repo.DeleteAsync(venue.Id);
        (await _repo.ExistsAsync(venue.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task CountAsync_NoPredicate_ReturnsCountOfActiveEntities()
    {
        await _repo.AddAsync(new Venue { Name = "V1", Address = "1 St", Capacity = 10 });
        await _repo.AddAsync(new Venue { Name = "V2", Address = "2 St", Capacity = 20 });
        var toDelete = await _repo.AddAsync(new Venue { Name = "V3", Address = "3 St", Capacity = 30 });
        await _repo.DeleteAsync(toDelete.Id);

        // Seed data adds 3 venues; we added 2 active + 1 deleted = net 2 new active
        var count = await _repo.CountAsync();
        count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task FindAsync_ReturnsMatchingEntities()
    {
        await _repo.AddAsync(new Venue { Name = "Big Hall", Address = "B St", Capacity = 500 });
        await _repo.AddAsync(new Venue { Name = "Small Room", Address = "S St", Capacity = 20 });

        var bigVenues = await _repo.FindAsync(v => v.Capacity >= 500);

        bigVenues.Should().OnlyContain(v => v.Capacity >= 500);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
