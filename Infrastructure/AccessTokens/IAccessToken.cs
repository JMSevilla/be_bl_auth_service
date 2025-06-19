using System.Security.Claims;
using LanguageExt;

namespace WTW.AuthenticationService.Infrastructure.AccessTokens;

public interface IAccessToken
{
    string Create(
        string userName,
        string businessGroup,
        string referenceNumber,
        string mainBusinessGroup,
        string mainReferenceNumber,
        Guid bereavementReferenceNumber,
        string tokenId);
    
    string Create(
        string userName,
        string businessGroup,
        string referenceNumber,
        Guid bereavementReferenceNumber,
        string tokenId);

    Try<ClaimsPrincipal> Validate(string accessToken);
}