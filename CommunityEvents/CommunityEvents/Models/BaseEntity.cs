namespace CommunityEvents.Models;

/// <summary>
/// Abstract base entity that all domain models inherit from.
/// Encapsulates common audit fields and enforces a contract via abstract members.
/// Demonstrates: Abstraction, Encapsulation, Inheritance.
/// </summary>
public abstract class BaseEntity
{
    private DateTime _createdAt;
    private DateTime _updatedAt;

    public int Id { get; set; }

    public DateTime CreatedAt
    {
        get => _createdAt;
        protected set => _createdAt = value;
    }

    public DateTime UpdatedAt
    {
        get => _updatedAt;
        set => _updatedAt = value;
    }

    public bool IsDeleted { get; protected set; }

    protected BaseEntity()
    {
        _createdAt = DateTime.UtcNow;
        _updatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Polymorphic display name — each subclass defines its own human-readable label.
    /// </summary>
    public abstract string DisplayName { get; }

    /// <summary>
    /// Polymorphic validation — each subclass enforces its own business rules.
    /// </summary>
    public abstract IEnumerable<string> Validate();

    /// <summary>Returns true only when Validate() produces no errors.</summary>
    public bool IsValid() => !Validate().Any();

    public virtual void SoftDelete()
    {
        IsDeleted = true;
        _updatedAt = DateTime.UtcNow;
    }

    public override string ToString() => $"[{GetType().Name}] Id={Id}, Name={DisplayName}";
}
