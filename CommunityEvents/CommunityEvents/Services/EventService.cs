using CommunityEvents.Data;
using CommunityEvents.Exceptions;
using CommunityEvents.Interfaces;
using CommunityEvents.Models;
using CommunityEvents.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CommunityEvents.Services;

/// <summary>
/// Implements IEventService — orchestrates the EventRepository and
/// applies the Strategy pattern for compound filtering.
/// </summary>
public class EventService : IEventService
{
    private readonly EventRepository _eventRepo;
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public EventService(
        EventRepository eventRepo,
        ApplicationDbContext context,
        INotificationService notificationService)
    {
        _eventRepo = eventRepo;
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<Event>> GetAllEventsAsync()
        => await _eventRepo.GetAllAsync();

    public async Task<IEnumerable<Event>> GetUpcomingEventsAsync()
        => await _eventRepo.GetUpcomingEventsAsync();

    public async Task<IEnumerable<Event>> FilterEventsAsync(
        DateTime? date,
        int? venueId,
        ActivityType? activityType,
        string? searchQuery)
    {
        // Build a composite strategy dynamically at runtime (Strategy + Composite pattern)
        var composite = new CompositeFilterStrategy();

        if (date.HasValue)
            composite.Add(new DateFilterStrategy(date.Value));
        if (venueId.HasValue && venueId > 0)
            composite.Add(new VenueFilterStrategy(venueId.Value));
        if (activityType.HasValue)
            composite.Add(new ActivityTypeFilterStrategy(activityType.Value));
        if (!string.IsNullOrWhiteSpace(searchQuery))
            composite.Add(new KeywordFilterStrategy(searchQuery));

        var baseQuery = _context.Events
            .Include(e => e.Venue)
            .Include(e => e.EventActivities).ThenInclude(ea => ea.Activity)
            .Where(e => !e.IsDeleted && e.Status == EventStatus.Published);

        return await composite.Apply(baseQuery)
            .OrderBy(e => e.EventDate)
            .ToListAsync();
    }

    public async Task<Event?> GetEventDetailsAsync(int id)
        => await _eventRepo.GetEventWithDetailsAsync(id);

    public async Task<Event> CreateEventAsync(Event evt)
    {
        var errors = evt.Validate().ToList();
        if (errors.Count > 0)
            throw new DomainValidationException(errors);

        return await _eventRepo.AddAsync(evt);
    }

    public async Task<Event> UpdateEventAsync(Event evt)
    {
        if (!await _eventRepo.ExistsAsync(evt.Id))
            throw new EventNotFoundException(evt.Id);

        var errors = evt.Validate().ToList();
        if (errors.Count > 0)
            throw new DomainValidationException(errors);

        await _eventRepo.UpdateAsync(evt);
        return evt;
    }

    public async Task PublishEventAsync(int id)
    {
        var evt = await _eventRepo.GetByIdAsync(id)
            ?? throw new EventNotFoundException(id);

        if (evt.Status == EventStatus.Cancelled)
            throw new InvalidStatusTransitionException(evt.Status.ToString(), EventStatus.Published.ToString());

        evt.Publish();
        await _eventRepo.UpdateAsync(evt);
        await _notificationService.NotifyEventPublishedAsync(evt);
    }

    public async Task CancelEventAsync(int id)
    {
        var evt = await _eventRepo.GetEventWithDetailsAsync(id)
            ?? throw new EventNotFoundException(id);

        evt.Cancel();
        await _eventRepo.UpdateAsync(evt);

        var confirmedParticipants = evt.Registrations
            .Where(r => r.Status == RegistrationStatus.Confirmed && r.Participant != null)
            .Select(r => r.Participant!)
            .ToList();

        if (confirmedParticipants.Count > 0)
            await _notificationService.NotifyEventCancelledAsync(evt, confirmedParticipants);
    }

    public async Task DeleteEventAsync(int id)
    {
        if (!await _eventRepo.ExistsAsync(id))
            throw new EventNotFoundException(id);
        await _eventRepo.DeleteAsync(id);
    }

    public async Task AddActivityToEventAsync(int eventId, int activityId, int displayOrder, TimeSpan? startTime)
    {
        var evt = await _eventRepo.GetByIdAsync(eventId)
            ?? throw new EventNotFoundException(eventId);

        var alreadyLinked = evt.EventActivities.Any(ea => ea.ActivityId == activityId);
        if (alreadyLinked)
            throw new DuplicateEntityException("EventActivity", $"Activity {activityId} is already part of Event {eventId}.");

        _context.EventActivities.Add(new EventActivity
        {
            EventId = eventId,
            ActivityId = activityId,
            DisplayOrder = displayOrder,
            ScheduledStartTime = startTime
        });
        await _context.SaveChangesAsync();
    }

    public async Task RemoveActivityFromEventAsync(int eventId, int activityId)
    {
        var link = await _context.EventActivities
            .FirstOrDefaultAsync(ea => ea.EventId == eventId && ea.ActivityId == activityId)
            ?? throw new NotFoundException("EventActivity", $"{eventId}/{activityId}");

        _context.EventActivities.Remove(link);
        await _context.SaveChangesAsync();
    }
}
