namespace CommunityEvents.Models;

/// <summary>
/// Join entity for the many-to-many relationship between Event and Activity.
/// Includes ordering metadata so activities can be sequenced within an event.
/// </summary>
public class EventActivity
{
    public int EventId { get; set; }
    public Event? Event { get; set; }

    public int ActivityId { get; set; }
    public Activity? Activity { get; set; }

    public int DisplayOrder { get; set; }

    public TimeSpan? ScheduledStartTime { get; set; }
}
