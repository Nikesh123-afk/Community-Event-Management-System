using CommunityEvents.Models;

namespace CommunityEvents.Interfaces;

/// <summary>
/// Observer-pattern interface: services that care about domain events implement this.
/// Demonstrates the Observer Design Pattern.
/// </summary>
public interface INotificationService
{
    Task NotifyRegistrationConfirmedAsync(Registration registration);
    Task NotifyEventCancelledAsync(Event evt, IEnumerable<Participant> affectedParticipants);
    Task NotifyEventPublishedAsync(Event evt);
}
