using System.ComponentModel.DataAnnotations;

namespace CommunityEvents.Models;

public enum RegistrationStatus
{
    Pending,
    Confirmed,
    Cancelled,
    WaitListed
}

/// <summary>
/// Junction entity representing the many-to-many relationship between
/// Participant and Event with additional metadata.
/// </summary>
public class Registration : BaseEntity
{
    // FKs
    [Required]
    public int ParticipantId { get; set; }
    public Participant? Participant { get; set; }

    [Required]
    public int EventId { get; set; }
    public Event? Event { get; set; }

    public RegistrationStatus Status { get; set; } = RegistrationStatus.Pending;

    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

    [StringLength(500)]
    public string? Notes { get; set; }

    public override string DisplayName =>
        $"Registration #{Id} — {Participant?.FullName ?? "Unknown"} for {Event?.Name ?? "Unknown Event"}";

    public override IEnumerable<string> Validate()
    {
        if (ParticipantId <= 0)
            yield return "A valid participant must be specified.";
        if (EventId <= 0)
            yield return "A valid event must be specified.";
    }

    public void Confirm()
    {
        Status = RegistrationStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = RegistrationStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void WaitList()
    {
        Status = RegistrationStatus.WaitListed;
        UpdatedAt = DateTime.UtcNow;
    }
}
