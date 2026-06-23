using CommunityEvents.Data;
using CommunityEvents.Models;
using Microsoft.EntityFrameworkCore;

namespace CommunityEvents.Repositories;

public class VenueRepository : BaseRepository<Venue>
{
    public VenueRepository(ApplicationDbContext context) : base(context) { }

    public override async Task<IEnumerable<Venue>> GetAllAsync()
        => await _dbSet
            .Where(v => !v.IsDeleted)
            .OrderBy(v => v.Name)
            .ToListAsync();
}
