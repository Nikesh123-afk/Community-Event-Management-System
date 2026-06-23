using System.ComponentModel.DataAnnotations;

namespace CommunityEvents.Models;

public enum ActivityType
{
    Workshop,
    Talk,
    Game,
    Performance,
    Networking,
    Exhibition,
    Other
}

/// <summary>
/// An activity that can be part of one or more events (many-to-many via EventActivity).
/// </summary>
public class Activity : BaseEntity
{
    [Required(ErrorMessage = "Activity name is required.")]
    [StringLength(150, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public ActivityType Type { get; set; } = ActivityType.Other;

    [Range(0, 480, ErrorMessage = "Duration must be 0–480 minutes.")]
    public int DurationMinutes { get; set; }

    // Navigation
    public ICollection<EventActivity> EventActivities { get; set; } = new List<EventActivity>();

    public override string DisplayName => $"{Name} ({Type})";

    public override IEnumerable<string> Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            yield return "Activity name cannot be empty.";
        if (DurationMinutes < 0)
            yield return "Duration cannot be negative.";
    }
}
