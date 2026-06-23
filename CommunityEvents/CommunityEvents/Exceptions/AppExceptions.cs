namespace CommunityEvents.Exceptions;

// ── Root ──────────────────────────────────────────────────────────────────────

/// <summary>
/// Root of the application exception hierarchy.
/// All domain-specific exceptions derive from this type.
/// </summary>
public abstract class AppException : Exception
{
    public string ErrorCode { get; }

    protected AppException(string message, string errorCode, Exception? inner = null)
        : base(message, inner)
    {
        ErrorCode = errorCode;
    }
}

// ── Validation ────────────────────────────────────────────────────────────────

public class ValidationException : AppException
{
    public string FieldName { get; }

    public ValidationException(string fieldName, string reason)
        : base($"Validation failed on '{fieldName}': {reason}", "VALIDATION_ERROR")
    {
        FieldName = fieldName;
    }
}

public class DomainValidationException : AppException
{
    public IReadOnlyList<string> Errors { get; }

    public DomainValidationException(IEnumerable<string> errors)
        : base($"Domain validation failed: {string.Join("; ", errors)}", "DOMAIN_VALIDATION_ERROR")
    {
        Errors = errors.ToList().AsReadOnly();
    }
}

// ── Not Found ─────────────────────────────────────────────────────────────────

public class NotFoundException : AppException
{
    public NotFoundException(string entityName, object id)
        : base($"{entityName} with identifier '{id}' was not found.", "NOT_FOUND") { }
}

public class EventNotFoundException : NotFoundException
{
    public EventNotFoundException(int id) : base("Event", id) { }
}

public class ParticipantNotFoundException : NotFoundException
{
    public ParticipantNotFoundException(int id) : base("Participant", id) { }
}

public class VenueNotFoundException : NotFoundException
{
    public VenueNotFoundException(int id) : base("Venue", id) { }
}

public class RegistrationNotFoundException : NotFoundException
{
    public RegistrationNotFoundException(int id) : base("Registration", id) { }
}

// ── Business Rules ────────────────────────────────────────────────────────────

public class BusinessRuleException : AppException
{
    public BusinessRuleException(string message)
        : base(message, "BUSINESS_RULE_VIOLATION") { }
}

public class EventCapacityExceededException : BusinessRuleException
{
    public EventCapacityExceededException(string eventName, int capacity)
        : base($"Event '{eventName}' is at full capacity ({capacity} spots).") { }
}

public class AlreadyRegisteredException : BusinessRuleException
{
    public AlreadyRegisteredException(string participantEmail, string eventName)
        : base($"'{participantEmail}' is already registered for '{eventName}'.") { }
}

public class EventAlreadyPassedException : BusinessRuleException
{
    public EventAlreadyPassedException(string eventName)
        : base($"Cannot register for '{eventName}' — the event date has passed.") { }
}

public class InvalidStatusTransitionException : BusinessRuleException
{
    public InvalidStatusTransitionException(string from, string to)
        : base($"Cannot transition event status from '{from}' to '{to}'.") { }
}

// ── Duplicate ─────────────────────────────────────────────────────────────────

public class DuplicateEntityException : AppException
{
    public DuplicateEntityException(string entityName, string detail)
        : base($"A duplicate {entityName} was found: {detail}", "DUPLICATE_ENTITY") { }
}
