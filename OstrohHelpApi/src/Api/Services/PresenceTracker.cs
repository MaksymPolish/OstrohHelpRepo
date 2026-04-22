using System.Collections.Concurrent;

namespace Api.Services;

public interface IPresenceTracker
{
    bool UserConnected(string userId, string connectionId);
    bool UserDisconnected(string userId, string connectionId);
    int GetConnectionCount(string userId);
}

public class PresenceTracker : IPresenceTracker
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _connections = new();

    public bool UserConnected(string userId, string connectionId)
    {
        var userConnections = _connections.GetOrAdd(
            userId,
            static _ => new ConcurrentDictionary<string, byte>());

        userConnections.TryAdd(connectionId, 0);

        // True only when this is the first active connection for the user.
        return userConnections.Count == 1;
    }

    public bool UserDisconnected(string userId, string connectionId)
    {
        if (!_connections.TryGetValue(userId, out var userConnections))
        {
            return false;
        }

        userConnections.TryRemove(connectionId, out _);

        if (!userConnections.IsEmpty)
        {
            return false;
        }

        _connections.TryRemove(userId, out _);
        return true;
    }

    public int GetConnectionCount(string userId)
    {
        return _connections.TryGetValue(userId, out var userConnections)
            ? userConnections.Count
            : 0;
    }
}
