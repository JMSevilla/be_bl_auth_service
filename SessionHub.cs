using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

[AllowAnonymous]
public class SessionHub : Hub
{
    private readonly SessionManager _sessionManager;

    public SessionHub(SessionManager sessionManager)
    {
        _sessionManager = sessionManager;
    }

    public async Task RegisterSession(string userName)
    {
        _sessionManager.RegisterUser(userName, Context.ConnectionId);
        await Clients.All.SendAsync("UserLoggedIn", userName);
    }

    public async Task UnregisterSession(string userName)
    {
        var connectionId = Context.ConnectionId;
        _sessionManager.UnregisterUserByConnectionId(connectionId);
        await Clients.All.SendAsync("UserLoggedOut", userName);
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        var connectionId = Context.ConnectionId;
        var userName = _sessionManager.GetUserNameByConnectionId(connectionId);
        if (userName != null)
        {
            _sessionManager.UnregisterUserByConnectionId(connectionId);
        }
        return base.OnDisconnectedAsync(exception);
    }
}