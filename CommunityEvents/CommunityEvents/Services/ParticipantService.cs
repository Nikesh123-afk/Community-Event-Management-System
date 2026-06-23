using CommunityEvents.Exceptions;
using CommunityEvents.Interfaces;
using CommunityEvents.Models;
using CommunityEvents.Repositories;

namespace CommunityEvents.Services;

public class ParticipantService : IParticipantService
{
    private readonly ParticipantRepository _repo;

    public ParticipantService(ParticipantRepository repo) => _repo = repo;

    public async Task<IEnumerable<Participant>> GetAllParticipantsAsync()
        => await _repo.GetAllAsync();

    public async Task<Participant?> GetParticipantDetailsAsync(int id)
        => await _repo.GetByIdAsync(id);

    public async Task<Participant?> GetParticipantByEmailAsync(string email)
        => await _repo.GetByEmailAsync(email);

    public async Task<Participant> CreateParticipantAsync(Participant participant)
    {
        var errors = participant.Validate().ToList();
        if (errors.Count > 0) throw new DomainValidationException(errors);

        var existing = await _repo.GetByEmailAsync(participant.Email);
        if (existing is not null)
            throw new DuplicateEntityException("Participant", $"email '{participant.Email}' already exists.");

        return await _repo.AddAsync(participant);
    }

    public async Task<Participant> UpdateParticipantAsync(Participant participant)
    {
        if (!await _repo.ExistsAsync(participant.Id))
            throw new ParticipantNotFoundException(participant.Id);

        var errors = participant.Validate().ToList();
        if (errors.Count > 0) throw new DomainValidationException(errors);

        await _repo.UpdateAsync(participant);
        return participant;
    }

    public async Task DeleteParticipantAsync(int id)
    {
        if (!await _repo.ExistsAsync(id)) throw new ParticipantNotFoundException(id);
        await _repo.DeleteAsync(id);
    }

    public async Task<IEnumerable<Registration>> GetParticipantRegistrationsAsync(int participantId)
    {
        var participant = await _repo.GetByIdAsync(participantId)
            ?? throw new ParticipantNotFoundException(participantId);
        return participant.Registrations;
    }
}
