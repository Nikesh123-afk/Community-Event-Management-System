using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CommunityEvents.Models;

/// <summary>
/// Represents a user who can register for community events.
/// Inherits from BaseEntity and overrides validation polymorphically.
/// </summary>
public class Participant : BaseEntity
{
    [Required(ErrorMessage = "First name is required.")]
    [StringLength(80, MinimumLength = 2)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(80, MinimumLength = 2)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "A valid email address is required.")]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "A valid phone number is required.")]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [StringLength(500)]
    public string? Bio { get; set; }

    // Linked to ASP.NET Core Identity user (optional — participant may register without an account)
    public string? ApplicationUserId { get; set; }

    // Navigation
    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();

    // Computed
    public string FullName => $"{FirstName} {LastName}";

    public override string DisplayName => FullName;

    public override IEnumerable<string> Validate()
    {
        if (string.IsNullOrWhiteSpace(FirstName))
            yield return "First name cannot be empty.";
        if (string.IsNullOrWhiteSpace(LastName))
            yield return "Last name cannot be empty.";
        if (string.IsNullOrWhiteSpace(Email))
            yield return "Email address cannot be empty.";
        if (!IsValidEmail(Email))
            yield return $"'{Email}' is not a valid email address.";
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        return Regex.IsMatch(email,
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
    }
}
