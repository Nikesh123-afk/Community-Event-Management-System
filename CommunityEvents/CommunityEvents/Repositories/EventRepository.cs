using CommunityEvents.Data;
using CommunityEvents.Interfaces;
using CommunityEvents.Models;
using Microsoft.EntityFrameworkCore;

namespace CommunityEvents.Repositories;

/// <summary>
/// Concrete repository for Event entities.
/// Inherits generic CRUD from BaseRepository and adds event-specific query methods.
/// </summary>
public class EventRepository : BaseRepository<Event>, IEventRepository
{
    public EventRepository(ApplicationDbContext context) : base(context) { }

    public override async Task<Event?> GetByIdAsync(int id)
        => await _dbSet
            .Include(e => e.Venue)
            .Include(e => e.Registrations).ThenInclude(r => r.Participant)
            .Include(e => e.EventActivities).ThenInclude(ea => ea.Activity)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

    public async Task<IEnumerable<Event>> GetUpcomingEventsAsync()
        => await _dbSet
            .Include(e => e.Venue)
            .Include(e => e.EventActivities).ThenInclude(ea => ea.Activity)
            .Where(e => !e.IsDeleted
                     && e.Status == EventStatus.Published
                     && e.EventDate >= DateTime.UtcNow.Date)
            .OrderBy(e => e.EventDate)
            .ToListAsync();

    public async Task<IEnumerable<Event>> GetEventsByVenueAsync(int venueId)
        => await _dbSet
            .Include(e => e.Venue)
            .Where(e => !e.IsDeleted && e.VenueId == venueId)
            .OrderBy(e => e.EventDate)
            .ToListAsync();

    public async Task<IEnumerable<Event>> GetEventsByDateRangeAsync(DateTime from, DateTime to)
        => await _dbSet
            .Include(e => e.Venue)
            .Where(e => !e.IsDeleted && e.EventDate >= from && e.EventDate <= to)
            .OrderBy(e => e.EventDate)
            .ToListAsync();

    public async Task<IEnumerable<Event>> GetEventsByActivityTypeAsync(ActivityType type)
        => await _dbSet
            .Include(e => e.Venue)
            .Include(e => e.EventActivities).ThenInclude(ea => ea.Activity)
            .Where(e => !e.IsDeleted
                     && e.EventActivities.Any(ea => ea.Activity != null && ea.Activity.Type == type))
            .OrderBy(e => e.EventDate)
            .ToListAsync();

    public async Task<Event?> GetEventWithDetailsAsync(int id)
        => await _dbSet
            .Include(e => e.Venue)
            .Include(e => e.Registrations).ThenInclude(r => r.Participant)
            .Include(e => e.EventActivities).ThenInclude(ea => ea.Activity)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

    public async Task<IEnumerable<Event>> SearchEventsAsync(string query)
    {
        var lower = query.ToLower();
        return await _dbSet
            .Include(e => e.Venue)
            .Where(e => !e.IsDeleted
                     && (e.Name.ToLower().Contains(lower)
                      || e.Description.ToLower().Contains(lower)
                      || (e.Venue != null && e.Venue.Name.ToLower().Contains(lower))))
            .OrderBy(e => e.EventDate)
            .ToListAsync();
    }
}
