using CommunityEvents.Interfaces;
using CommunityEvents.Models;

namespace CommunityEvents.Services;

/// <summary>
/// Concrete Strategy: filters events by a specific date.
/// </summary>
public class DateFilterStrategy : IFilterStrategy
{
    private readonly DateTime _date;
    public DateFilterStrategy(DateTime date) => _date = date.Date;

    public IQueryable<Event> Apply(IQueryable<Event> query)
        => query.Where(e => e.EventDate.Date == _date);
}

/// <summary>
/// Concrete Strategy: filters events by venue.
/// </summary>
public class VenueFilterStrategy : IFilterStrategy
{
    private readonly int _venueId;
    public VenueFilterStrategy(int venueId) => _venueId = venueId;

    public IQueryable<Event> Apply(IQueryable<Event> query)
        => query.Where(e => e.VenueId == _venueId);
}

/// <summary>
/// Concrete Strategy: filters events by activity type.
/// </summary>
public class ActivityTypeFilterStrategy : IFilterStrategy
{
    private readonly ActivityType _type;
    public ActivityTypeFilterStrategy(ActivityType type) => _type = type;

    public IQueryable<Event> Apply(IQueryable<Event> query)
        => query.Where(e => e.EventActivities.Any(ea => ea.Activity != null && ea.Activity.Type == _type));
}

/// <summary>
/// Concrete Strategy: full-text keyword search across name, description, venue name.
/// </summary>
public class KeywordFilterStrategy : IFilterStrategy
{
    private readonly string _keyword;
    public KeywordFilterStrategy(string keyword) => _keyword = keyword.ToLower();

    public IQueryable<Event> Apply(IQueryable<Event> query)
        => query.Where(e =>
            e.Name.ToLower().Contains(_keyword) ||
            e.Description.ToLower().Contains(_keyword) ||
            (e.Venue != null && e.Venue.Name.ToLower().Contains(_keyword)));
}

/// <summary>
/// Composite Strategy: chains multiple filter strategies together (AND logic).
/// Demonstrates the Composite Design Pattern combined with Strategy.
/// </summary>
public class CompositeFilterStrategy : IFilterStrategy
{
    private readonly List<IFilterStrategy> _strategies = new();

    public CompositeFilterStrategy Add(IFilterStrategy strategy)
    {
        _strategies.Add(strategy);
        return this; // fluent interface
    }

    public IQueryable<Event> Apply(IQueryable<Event> query)
        => _strategies.Aggregate(query, (current, strategy) => strategy.Apply(current));
}
