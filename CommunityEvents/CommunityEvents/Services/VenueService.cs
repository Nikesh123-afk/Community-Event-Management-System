using CommunityEvents.Exceptions;
using CommunityEvents.Interfaces;
using CommunityEvents.Models;
using CommunityEvents.Repositories;

namespace CommunityEvents.Services;

public class VenueService : IVenueService
{
    private readonly VenueRepository _repo;

    public VenueService(VenueRepository repo) => _repo = repo;

    public async Task<IEnumerable<Venue>> GetAllVenuesAsync() => await _repo.GetAllAsync();

    public async Task<Venue?> GetVenueDetailsAsync(int id) => await _repo.GetByIdAsync(id);

    public async Task<Venue> CreateVenueAsync(Venue venue)
    {
        var errors = venue.Validate().ToList();
        if (errors.Count > 0) throw new DomainValidationException(errors);
        return await _repo.AddAsync(venue);
    }

    public async Task<Venue> UpdateVenueAsync(Venue venue)
    {
        if (!await _repo.ExistsAsync(venue.Id)) throw new VenueNotFoundException(venue.Id);
        var errors = venue.Validate().ToList();
        if (errors.Count > 0) throw new DomainValidationException(errors);
        await _repo.UpdateAsync(venue);
        return venue;
    }

    public async Task DeleteVenueAsync(int id)
    {
        if (!await _repo.ExistsAsync(id)) throw new VenueNotFoundException(id);
        await _repo.DeleteAsync(id);
    }
}
