namespace ChatApp.Services;

using System.Collections.Concurrent;
using ChatApp.Services.Interfaces;

public class InMemoryPresenceTracker : IPresenceTracker
{
    private static readonly ConcurrentDictionary<Guid, HashSet<string>> _onlineUsers = new();
    private static readonly object _lock = new();

    public Task<bool> TrackConnectionAsync(Guid userId, string connectionId)
    {
        bool isNewlyOnline = false;

        lock (_lock)
        {
            if (_onlineUsers.TryGetValue(userId, out var connections))
            {
                connections.Add(connectionId);
            }
            else
            {
                _onlineUsers[userId] = new HashSet<string> { connectionId };
                isNewlyOnline = true;
            }
        }

        return Task.FromResult(isNewlyOnline);
    }

    public Task<bool> UntrackConnectionAsync(Guid userId, string connectionId)
    {
        bool isNowOffline = false;

        lock (_lock)
        {
            if (_onlineUsers.TryGetValue(userId, out var connections))
            {
                connections.Remove(connectionId);

                if (connections.Count == 0)
                {
                    _onlineUsers.TryRemove(userId, out _);
                    isNowOffline = true;
                }
            }
        }

        return Task.FromResult(isNowOffline);
    }

    public Task<List<Guid>> GetOnlineUsersAsync()
    {
        List<Guid> onlineUserIds;

        lock (_lock)
        {
            onlineUserIds = _onlineUsers.Keys.ToList();
        }

        return Task.FromResult(onlineUserIds);
    }

    public Task<List<string>> GetConnectionsForUserAsync(Guid userId)
    {
        List<string> connectionIds;

        lock (_lock)
        {
            if (_onlineUsers.TryGetValue(userId, out var connections))
            {
                connectionIds = connections.ToList();
            }
            else
            {
                connectionIds = new List<string>();
            }
        }

        return Task.FromResult(connectionIds);
    }
}
