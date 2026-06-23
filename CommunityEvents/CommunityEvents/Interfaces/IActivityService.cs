using CommunityEvents.Models;

namespace CommunityEvents.Interfaces;

public interface IActivityService
{
    Task<IEnumerable<Activity>> GetAllActivitiesAsync();
    Task<Activity?> GetActivityDetailsAsync(int id);
    Task<Activity> CreateActivityAsync(Activity activity);
    Task<Activity> UpdateActivityAsync(Activity activity);
    Task DeleteActivityAsync(int id);
}
