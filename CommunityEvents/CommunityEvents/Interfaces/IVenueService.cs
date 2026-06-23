using CommunityEvents.Models;

namespace CommunityEvents.Interfaces;

public interface IVenueService
{
    Task<IEnumerable<Venue>> GetAllVenuesAsync();
    Task<Venue?> GetVenueDetailsAsync(int id);
    Task<Venue> CreateVenueAsync(Venue venue);
    Task<Venue> UpdateVenueAsync(Venue venue);
    Task DeleteVenueAsync(int id);
}
