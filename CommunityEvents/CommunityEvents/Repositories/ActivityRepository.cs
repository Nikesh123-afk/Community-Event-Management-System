using CommunityEvents.Data;
using CommunityEvents.Models;
using Microsoft.EntityFrameworkCore;

namespace CommunityEvents.Repositories;

public class ActivityRepository : BaseRepository<Activity>
{
    public ActivityRepository(ApplicationDbContext context) : base(context) { }

    public override async Task<IEnumerable<Activity>> GetAllAsync()
        => await _dbSet.Where(a => !a.IsDeleted).OrderBy(a => a.Name).ToListAsync();
}
