using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityEvents.Models;

public enum EventStatus
{
    Draft,
    Published,
    Cancelled,
    Completed
}

public enum EventType
{
    InPerson,
    Virtual,
    Hybrid
}

/// <summary>
/// Core domain model. Demonstrates inheritance (from BaseEntity), encapsulation
/// (private backing fields + properties), and polymorphism (override Validate/DisplayName).
/// </summary>
public class Event : BaseEntity
{
    private DateTime _eventDate;
    private int _maxCapacity;

    [Required(ErrorMessage = "Event name is required.")]
    [StringLength(200, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(2000, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;

    public DateTime EventDate
    {
        get => _eventDate;
        set
        {
            if (value < DateTime.UtcNow.Date && Status == EventStatus.Draft)
                throw new Exceptions.ValidationException(nameof(EventDate), "Event date cannot be in the past for a new draft.");
            _eventDate = value;
        }
    }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    public EventStatus Status { get; set; } = EventStatus.Draft;
    public EventType Type { get; set; } = EventType.InPerson;

    public int MaxCapacity
    {
        get => _maxCapacity;
        set
        {
            if (value <= 0)
                throw new Exceptions.ValidationException(nameof(MaxCapacity), "Max capacity must be positive.");
            _maxCapacity = value;
        }
    }

    // FK + Navigation
    public int VenueId { get; set; }
    public Venue? Venue { get; set; }

    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
    public ICollection<EventActivity> EventActivities { get; set; } = new List<EventActivity>();

    // Computed / read-only business logic
    [NotMapped]
    public int ConfirmedRegistrationCount =>
        Registrations.Count(r => r.Status == RegistrationStatus.Confirmed);

    [NotMapped]
    public int AvailableSpots => MaxCapacity - ConfirmedRegistrationCount;

    [NotMapped]
    public bool IsFull => AvailableSpots <= 0;

    [NotMapped]
    public bool HasPassed => EventDate.Date < DateTime.UtcNow.Date;

    [NotMapped]
    public bool IsUpcoming => EventDate.Date >= DateTime.UtcNow.Date && Status == EventStatus.Published;

    // Polymorphic overrides
    public override string DisplayName => $"{Name} — {EventDate:dd MMM yyyy}";

    public override IEnumerable<string> Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            yield return "Event name cannot be empty.";
        if (string.IsNullOrWhiteSpace(Description))
            yield return "Description cannot be empty.";
        if (MaxCapacity <= 0)
            yield return "Max capacity must be greater than zero.";
        if (EndTime <= StartTime)
            yield return "End time must be after start time.";
        if (VenueId <= 0)
            yield return "A venue must be selected.";
    }

    /// <summary>Publishes the event if it is currently a draft and passes validation.</summary>
    public void Publish()
    {
        var errors = Validate().ToList();
        if (errors.Count > 0)
            throw new Exceptions.DomainValidationException(errors);
        Status = EventStatus.Published;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = EventStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkCompleted()
    {
        Status = EventStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }
}
