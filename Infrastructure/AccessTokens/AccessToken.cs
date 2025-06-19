using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LanguageExt;
using Microsoft.IdentityModel.Tokens;
using WTW.Web.Authentication;

namespace WTW.AuthenticationService.Infrastructure.AccessTokens;

public class AccessToken : IAccessToken
{
    private readonly AuthenticationSettings _settings;
    private readonly IAccessTokenHelper _accessTokenHelper;

    public AccessToken(AuthenticationSettings settings, IAccessTokenHelper accessTokenHelper)
    {
        _settings = settings;
        _accessTokenHelper = accessTokenHelper;
    }

    public string Create(
        string userName,
        string businessGroup,
        string referenceNumber,
        string mainBusinessGroup,
        string mainReferenceNumber,
        Guid bereavementReferenceNumber,
        string tokenId)
    {
        var claims = _accessTokenHelper.CreateClaims(userName, businessGroup, referenceNumber, mainBusinessGroup, mainReferenceNumber, bereavementReferenceNumber, tokenId);

        return WriteToken(claims);
    }

    public string Create(
        string userName,
        string businessGroup,
        string referenceNumber,
        Guid bereavementReferenceNumber,
        string tokenId)
    {
        return Create(userName, businessGroup, referenceNumber, businessGroup, referenceNumber, bereavementReferenceNumber, tokenId);
    }

    public Try<ClaimsPrincipal> Validate(string accessToken)
    {
        var validationParams = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key)),
            ValidAlgorithms = new string[] { SecurityAlgorithms.HmacSha256 },
            ValidateLifetime = false,
            ValidAudience = _settings.Audience,
            ValidIssuer = _settings.Issuer,
        };

        return () =>
        {
            var principal = new JwtSecurityTokenHandler().ValidateToken(
                accessToken,
                validationParams,
                out var securityToken);

            return principal;
        };
    }

    private string WriteToken(IEnumerable<Claim> claims)
    {
        return _accessTokenHelper.GenerateJwtToken(
            claims,
            _settings.Key,
            _settings.Issuer,
            _settings.Audience,
            _settings.ExpiresInMin);
    }
}
