using CommunityEvents.Exceptions;
using CommunityEvents.Interfaces;
using CommunityEvents.Models;
using CommunityEvents.Repositories;

namespace CommunityEvents.Services;

public class ActivityService : IActivityService
{
    private readonly ActivityRepository _repo;

    public ActivityService(ActivityRepository repo) => _repo = repo;

    public async Task<IEnumerable<Activity>> GetAllActivitiesAsync() => await _repo.GetAllAsync();
    public async Task<Activity?> GetActivityDetailsAsync(int id) => await _repo.GetByIdAsync(id);

    public async Task<Activity> CreateActivityAsync(Activity activity)
    {
        var errors = activity.Validate().ToList();
        if (errors.Count > 0) throw new DomainValidationException(errors);
        return await _repo.AddAsync(activity);
    }

    public async Task<Activity> UpdateActivityAsync(Activity activity)
    {
        if (!await _repo.ExistsAsync(activity.Id)) throw new NotFoundException("Activity", activity.Id);
        var errors = activity.Validate().ToList();
        if (errors.Count > 0) throw new DomainValidationException(errors);
        await _repo.UpdateAsync(activity);
        return activity;
    }

    public async Task DeleteActivityAsync(int id)
    {
        if (!await _repo.ExistsAsync(id)) throw new NotFoundException("Activity", id);
        await _repo.DeleteAsync(id);
    }
}
