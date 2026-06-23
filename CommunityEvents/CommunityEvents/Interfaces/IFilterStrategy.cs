using CommunityEvents.Models;

namespace CommunityEvents.Interfaces;

/// <summary>
/// Strategy pattern interface for event filtering.
/// Allows swapping algorithms at runtime without changing caller code.
/// </summary>
public interface IFilterStrategy
{
    IQueryable<Event> Apply(IQueryable<Event> query);
}
