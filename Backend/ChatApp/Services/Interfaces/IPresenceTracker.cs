namespace ChatApp.Services.Interfaces;

public interface IPresenceTracker
{
    Task<bool> TrackConnectionAsync(Guid userId, string connectionId);
    Task<bool> UntrackConnectionAsync(Guid userId, string connectionId);
    Task<List<Guid>> GetOnlineUsersAsync();
    Task<List<string>> GetConnectionsForUserAsync(Guid userId);
}
