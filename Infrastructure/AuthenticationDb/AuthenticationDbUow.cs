
namespace WTW.AuthenticationService.Infrastructure.AuthenticationDb;

public class AuthenticationDbUow : IAuthenticationDbUow
{
    private readonly AuthenticationDbContext _context;

    public AuthenticationDbUow(AuthenticationDbContext context, IRefreshTokenRepository tokens)
    {
        _context = context;
        Tokens = tokens;
    }

    public IRefreshTokenRepository Tokens { get; }

    public void Add(object entity)
    {
        _context.Add(entity);
    }

    public void Remove(object entity)
    {
        _context.Remove(entity);
    }

    public async Task Commit()
    {
        await _context.SaveChangesAsync();
    }
}