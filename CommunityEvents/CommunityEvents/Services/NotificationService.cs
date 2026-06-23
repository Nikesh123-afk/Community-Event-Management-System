using CommunityEvents.Interfaces;
using CommunityEvents.Models;
using Microsoft.Extensions.Logging;

namespace CommunityEvents.Services;

/// <summary>
/// Observer-pattern concrete implementation.
/// In production this would send emails / push notifications.
/// Here it logs the events for demo purposes and can be swapped via DI.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger)
        => _logger = logger;

    public Task NotifyRegistrationConfirmedAsync(Registration registration)
    {
        _logger.LogInformation(
            "NOTIFICATION ► Registration confirmed: {Participant} → {Event} (Reg #{Id})",
            registration.Participant?.FullName ?? "Unknown",
            registration.Event?.Name ?? "Unknown",
            registration.Id);

        return Task.CompletedTask;
    }

    public Task NotifyEventCancelledAsync(Event evt, IEnumerable<Participant> affectedParticipants)
    {
        foreach (var p in affectedParticipants)
            _logger.LogWarning(
                "NOTIFICATION ► Event cancelled: {Event} on {Date}. Notifying {Participant} ({Email}).",
                evt.Name, evt.EventDate.ToShortDateString(), p.FullName, p.Email);

        return Task.CompletedTask;
    }

    public Task NotifyEventPublishedAsync(Event evt)
    {
        _logger.LogInformation(
            "NOTIFICATION ► Event published: {Event} on {Date} at {Venue}.",
            evt.Name, evt.EventDate.ToShortDateString(), evt.Venue?.Name ?? "TBA");

        return Task.CompletedTask;
    }
}
