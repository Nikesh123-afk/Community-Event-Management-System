using CommunityEvents.Data;
using CommunityEvents.Models;
using Microsoft.EntityFrameworkCore;

namespace CommunityEvents.Repositories;

public class ParticipantRepository : BaseRepository<Participant>
{
    public ParticipantRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Participant?> GetByEmailAsync(string email)
        => await _dbSet
            .Include(p => p.Registrations).ThenInclude(r => r.Event)
            .FirstOrDefaultAsync(p => p.Email.ToLower() == email.ToLower() && !p.IsDeleted);

    public override async Task<Participant?> GetByIdAsync(int id)
        => await _dbSet
            .Include(p => p.Registrations).ThenInclude(r => r.Event).ThenInclude(e => e!.Venue)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
}
