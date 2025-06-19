using System.Security.Claims;
using LanguageExt;

namespace WTW.AuthenticationService.Infrastructure.AccessTokens;

public interface IBereavementToken
{
    string CreateInitial(string businessGroup, Guid bereavementReferenceNumber);
    string CreateWithEmailVerifiedRole(string businessGroup, Guid bereavementReferenceNumber);
    Try<ClaimsPrincipal> Validate(string accessToken);
}