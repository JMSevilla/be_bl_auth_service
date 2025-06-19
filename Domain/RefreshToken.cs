namespace WTW.AuthenticationService.Domain;

public class RefreshToken
{
    protected RefreshToken() { }

    public RefreshToken(string userId, string sessionId, string value, DateTimeOffset expiresAt)
    {
        Id = 0;
        UserId = userId;
        SessionId = sessionId;
        Value = value;
        ExpiresAt = expiresAt;
    }

    public int Id { get; }
    public string UserId { get; }
    public string SessionId { get; }
    public string Value { get; }
    public DateTimeOffset ExpiresAt { get; }

    public bool IsActive(DateTimeOffset now)
    {
        return ExpiresAt > now;
    }
}