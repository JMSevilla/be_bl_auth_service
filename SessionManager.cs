using System.Collections.Concurrent;

public class SessionManager
{
    private readonly ConcurrentDictionary<string, string> _loggedInUsers = new();

    public bool IsUserLoggedIn(string userName)
    {
        return _loggedInUsers.Values.Contains(userName);
    }

    public void RegisterUser(string userName, string connectionId)
    {
        _loggedInUsers[connectionId] = userName;
    }

    public void UnregisterUserByConnectionId(string connectionId)
    {
        _loggedInUsers.TryRemove(connectionId, out _);
    }

    public string GetUserNameByConnectionId(string connectionId)
    {
        _loggedInUsers.TryGetValue(connectionId, out var userName);
        return userName;
    }
}