using CommunityEvents.Models;

namespace CommunityEvents.Interfaces;

/// <summary>Business-logic contract for event management operations.</summary>
public interface IEventService
{
    Task<IEnumerable<Event>> GetAllEventsAsync();
    Task<IEnumerable<Event>> GetUpcomingEventsAsync();
    Task<IEnumerable<Event>> FilterEventsAsync(DateTime? date, int? venueId, ActivityType? activityType, string? searchQuery);
    Task<Event?> GetEventDetailsAsync(int id);
    Task<Event> CreateEventAsync(Event evt);
    Task<Event> UpdateEventAsync(Event evt);
    Task PublishEventAsync(int id);
    Task CancelEventAsync(int id);
    Task DeleteEventAsync(int id);
    Task AddActivityToEventAsync(int eventId, int activityId, int displayOrder, TimeSpan? startTime);
    Task RemoveActivityFromEventAsync(int eventId, int activityId);
}
