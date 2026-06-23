using CommunityEvents.Models;

namespace CommunityEvents.Interfaces;

public interface IRegistrationService
{
    Task<Registration> RegisterParticipantAsync(int participantId, int eventId, string? notes = null);
    Task<Registration> CancelRegistrationAsync(int registrationId);
    Task<Registration> ConfirmRegistrationAsync(int registrationId);
    Task<IEnumerable<Registration>> GetRegistrationsForEventAsync(int eventId);
    Task<IEnumerable<Registration>> GetRegistrationsForParticipantAsync(int participantId);
    Task<bool> IsParticipantRegisteredAsync(int participantId, int eventId);
}
