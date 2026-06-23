using System.ComponentModel.DataAnnotations;

namespace CommunityEvents.Models;

/// <summary>
/// Represents a physical or virtual location where events are held.
/// Inherits audit fields from BaseEntity; overrides DisplayName and Validate (polymorphism).
/// </summary>
public class Venue : BaseEntity
{
    [Required(ErrorMessage = "Venue name is required.")]
    [StringLength(150, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address is required.")]
    [StringLength(300)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [Range(1, 100_000, ErrorMessage = "Capacity must be between 1 and 100,000.")]
    public int Capacity { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [Phone]
    public string? ContactPhone { get; set; }

    [EmailAddress]
    public string? ContactEmail { get; set; }

    // Navigation
    public ICollection<Event> Events { get; set; } = new List<Event>();

    public override string DisplayName => Name;

    public override IEnumerable<string> Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            yield return "Venue name cannot be empty.";
        if (string.IsNullOrWhiteSpace(Address))
            yield return "Address cannot be empty.";
        if (Capacity <= 0)
            yield return "Capacity must be a positive integer.";
    }

    /// <summary>Returns true when the venue can accommodate the given number of guests.</summary>
    public bool CanAccommodate(int attendeeCount) => attendeeCount <= Capacity;
}
