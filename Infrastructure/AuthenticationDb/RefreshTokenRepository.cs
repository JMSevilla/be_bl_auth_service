using LanguageExt;
using Microsoft.EntityFrameworkCore;
using WTW.AuthenticationService.Domain;

namespace WTW.AuthenticationService.Infrastructure.AuthenticationDb
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AuthenticationDbContext _context;

        public RefreshTokenRepository(AuthenticationDbContext context)
        {
            _context = context;
        }

        public async Task<Option<RefreshToken>> Find(string token)
        {
            return await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Value == token);
        }

        public async Task<Option<RefreshToken>> FindBySessionId(string sessionId)
        {
            return await _context.RefreshTokens.FirstOrDefaultAsync(x => x.SessionId == sessionId);
        }
    }
}