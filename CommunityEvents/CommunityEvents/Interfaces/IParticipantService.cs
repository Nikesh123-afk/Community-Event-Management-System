using CommunityEvents.Models;

namespace CommunityEvents.Interfaces;

public interface IParticipantService
{
    Task<IEnumerable<Participant>> GetAllParticipantsAsync();
    Task<Participant?> GetParticipantDetailsAsync(int id);
    Task<Participant?> GetParticipantByEmailAsync(string email);
    Task<Participant> CreateParticipantAsync(Participant participant);
    Task<Participant> UpdateParticipantAsync(Participant participant);
    Task DeleteParticipantAsync(int id);
    Task<IEnumerable<Registration>> GetParticipantRegistrationsAsync(int participantId);
}
