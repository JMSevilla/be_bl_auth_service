using WTW.AuthenticationService.Domain;

namespace WTW.AuthenticationService.Tokens;

public interface IRefreshTokenFactory
{
    RefreshToken Create(
        string userId,
        string sessionId,
        DateTimeOffset now);
}