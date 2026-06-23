using CommunityEvents.Models;

namespace CommunityEvents.Factories;

/// <summary>
/// Factory pattern: centralises Event creation so that all events are born in
/// a valid, consistently initialised state without callers knowing the internals.
/// Demonstrates the Factory Design Pattern.
/// </summary>
public static class EventFactory
{
    /// <summary>Creates a standard in-person community event draft.</summary>
    public static Event CreateInPersonEvent(
        string name,
        string description,
        DateTime date,
        TimeSpan start,
        TimeSpan end,
        int venueId,
        int maxCapacity)
    {
        ValidateCommonFields(name, description, date, maxCapacity);

        return new Event
        {
            Name = name.Trim(),
            Description = description.Trim(),
            EventDate = date,
            StartTime = start,
            EndTime = end,
            VenueId = venueId,
            MaxCapacity = maxCapacity,
            Type = EventType.InPerson,
            Status = EventStatus.Draft
        };
    }

    /// <summary>Creates a virtual (online) event — venue is optional.</summary>
    public static Event CreateVirtualEvent(
        string name,
        string description,
        DateTime date,
        TimeSpan start,
        TimeSpan end,
        int maxCapacity,
        int venueId = 0)
    {
        ValidateCommonFields(name, description, date, maxCapacity);

        return new Event
        {
            Name = name.Trim(),
            Description = description.Trim(),
            EventDate = date,
            StartTime = start,
            EndTime = end,
            VenueId = venueId > 0 ? venueId : 1,
            MaxCapacity = maxCapacity,
            Type = EventType.Virtual,
            Status = EventStatus.Draft
        };
    }

    /// <summary>Creates a hybrid event (in-person + online stream).</summary>
    public static Event CreateHybridEvent(
        string name,
        string description,
        DateTime date,
        TimeSpan start,
        TimeSpan end,
        int venueId,
        int maxCapacity)
    {
        ValidateCommonFields(name, description, date, maxCapacity);

        return new Event
        {
            Name = name.Trim(),
            Description = description.Trim(),
            EventDate = date,
            StartTime = start,
            EndTime = end,
            VenueId = venueId,
            MaxCapacity = maxCapacity,
            Type = EventType.Hybrid,
            Status = EventStatus.Draft
        };
    }

    private static void ValidateCommonFields(string name, string description, DateTime date, int maxCapacity)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Exceptions.ValidationException(nameof(name), "Event name cannot be empty.");
        if (string.IsNullOrWhiteSpace(description))
            throw new Exceptions.ValidationException(nameof(description), "Description cannot be empty.");
        if (maxCapacity <= 0)
            throw new Exceptions.ValidationException(nameof(maxCapacity), "Max capacity must be positive.");
    }
}
