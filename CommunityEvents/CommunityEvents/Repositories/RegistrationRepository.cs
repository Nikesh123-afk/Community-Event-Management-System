using CommunityEvents.Data;
using CommunityEvents.Models;
using Microsoft.EntityFrameworkCore;

namespace CommunityEvents.Repositories;

public class RegistrationRepository : BaseRepository<Registration>
{
    public RegistrationRepository(ApplicationDbContext context) : base(context) { }

    public override async Task<Registration?> GetByIdAsync(int id)
        => await _dbSet
            .Include(r => r.Participant)
            .Include(r => r.Event).ThenInclude(e => e!.Venue)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

    public async Task<IEnumerable<Registration>> GetByEventIdAsync(int eventId)
        => await _dbSet
            .Include(r => r.Participant)
            .Where(r => r.EventId == eventId && !r.IsDeleted)
            .OrderBy(r => r.RegistrationDate)
            .ToListAsync();

    public async Task<IEnumerable<Registration>> GetByParticipantIdAsync(int participantId)
        => await _dbSet
            .Include(r => r.Event).ThenInclude(e => e!.Venue)
            .Where(r => r.ParticipantId == participantId && !r.IsDeleted)
            .OrderByDescending(r => r.RegistrationDate)
            .ToListAsync();

    public async Task<bool> ExistsDuplicateAsync(int participantId, int eventId)
        => await _dbSet.AnyAsync(r =>
            r.ParticipantId == participantId &&
            r.EventId == eventId &&
            r.Status != RegistrationStatus.Cancelled &&
            !r.IsDeleted);
}
