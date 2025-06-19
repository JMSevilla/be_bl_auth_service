namespace WTW.AuthenticationService.Infrastructure.AuthenticationDb;

public interface IAuthenticationDbUow
{
    IRefreshTokenRepository Tokens { get; }
    void Add(object entity);
    void Remove(object entity);
    Task Commit();
}