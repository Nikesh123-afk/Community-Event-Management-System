using CommunityEvents.Exceptions;
using CommunityEvents.Interfaces;
using CommunityEvents.Models;
using CommunityEvents.Repositories;

namespace CommunityEvents.Services;

/// <summary>
/// Orchestrates registration lifecycle, enforcing all business rules:
/// capacity checks, duplicate detection, event-date validation, wait-listing.
/// </summary>
public class RegistrationService : IRegistrationService
{
    private readonly RegistrationRepository _regRepo;
    private readonly EventRepository _eventRepo;
    private readonly ParticipantRepository _participantRepo;
    private readonly INotificationService _notificationService;

    public RegistrationService(
        RegistrationRepository regRepo,
        EventRepository eventRepo,
        ParticipantRepository participantRepo,
        INotificationService notificationService)
    {
        _regRepo = regRepo;
        _eventRepo = eventRepo;
        _participantRepo = participantRepo;
        _notificationService = notificationService;
    }

    public async Task<Registration> RegisterParticipantAsync(int participantId, int eventId, string? notes = null)
    {
        var evt = await _eventRepo.GetEventWithDetailsAsync(eventId)
            ?? throw new EventNotFoundException(eventId);

        var participant = await _participantRepo.GetByIdAsync(participantId)
            ?? throw new ParticipantNotFoundException(participantId);

        // Business rule: cannot register for a past event
        if (evt.HasPassed)
            throw new EventAlreadyPassedException(evt.Name);

        // Business rule: no duplicate registrations
        if (await _regRepo.ExistsDuplicateAsync(participantId, eventId))
            throw new AlreadyRegisteredException(participant.Email, evt.Name);

        var registration = new Registration
        {
            ParticipantId = participantId,
            EventId = eventId,
            RegistrationDate = DateTime.UtcNow,
            Notes = notes
        };

        // Determine status: confirmed or wait-listed depending on capacity
        if (evt.IsFull)
            registration.WaitList();
        else
            registration.Confirm();

        await _regRepo.AddAsync(registration);

        if (registration.Status == RegistrationStatus.Confirmed)
            await _notificationService.NotifyRegistrationConfirmedAsync(registration);

        return registration;
    }

    public async Task<Registration> CancelRegistrationAsync(int registrationId)
    {
        var registration = await _regRepo.GetByIdAsync(registrationId)
            ?? throw new RegistrationNotFoundException(registrationId);

        registration.Cancel();
        await _regRepo.UpdateAsync(registration);
        return registration;
    }

    public async Task<Registration> ConfirmRegistrationAsync(int registrationId)
    {
        var registration = await _regRepo.GetByIdAsync(registrationId)
            ?? throw new RegistrationNotFoundException(registrationId);

        registration.Confirm();
        await _regRepo.UpdateAsync(registration);
        await _notificationService.NotifyRegistrationConfirmedAsync(registration);
        return registration;
    }

    public async Task<IEnumerable<Registration>> GetRegistrationsForEventAsync(int eventId)
        => await _regRepo.GetByEventIdAsync(eventId);

    public async Task<IEnumerable<Registration>> GetRegistrationsForParticipantAsync(int participantId)
        => await _regRepo.GetByParticipantIdAsync(participantId);

    public async Task<bool> IsParticipantRegisteredAsync(int participantId, int eventId)
        => await _regRepo.ExistsDuplicateAsync(participantId, eventId);
}
