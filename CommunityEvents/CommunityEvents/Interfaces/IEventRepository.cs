using CommunityEvents.Models;

namespace CommunityEvents.Interfaces;

/// <summary>Extends the generic repository with event-specific queries.</summary>
public interface IEventRepository : IRepository<Event>
{
    Task<IEnumerable<Event>> GetUpcomingEventsAsync();
    Task<IEnumerable<Event>> GetEventsByVenueAsync(int venueId);
    Task<IEnumerable<Event>> GetEventsByDateRangeAsync(DateTime from, DateTime to);
    Task<IEnumerable<Event>> GetEventsByActivityTypeAsync(ActivityType type);
    Task<Event?> GetEventWithDetailsAsync(int id);
    Task<IEnumerable<Event>> SearchEventsAsync(string query);
}
