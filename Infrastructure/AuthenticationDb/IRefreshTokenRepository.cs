using LanguageExt;
using WTW.AuthenticationService.Domain;

namespace WTW.AuthenticationService.Infrastructure.AuthenticationDb;

public interface IRefreshTokenRepository
{
    Task<Option<RefreshToken>> Find(string token);
    Task<Option<RefreshToken>> FindBySessionId(string sessionId);
}