using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LanguageExt;
using Microsoft.IdentityModel.Tokens;
using WTW.Web.Authentication;

namespace WTW.AuthenticationService.Infrastructure.AccessTokens;

public class BereavementToken : IBereavementToken
{
    private readonly AuthenticationSettings _settings;

    public BereavementToken(AuthenticationSettings settings)
    {
        _settings = settings;
    }

    public string CreateInitial(string businessGroup, Guid bereavementReferenceNumber)
    {
        var claims = CreateInitialClaims(businessGroup, bereavementReferenceNumber);

        return WriteToken(claims);
    }

    public string CreateWithEmailVerifiedRole(string businessGroup, Guid bereavementReferenceNumber)
    {
        var claims = CreateInitialClaims(businessGroup, bereavementReferenceNumber);
        claims.Add(new Claim(ClaimTypes.Role, "BereavementEmailVerifiedUser"));

        return WriteToken(claims);
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
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _settings.Issuer,
            _settings.Audience,
            claims,
            expires: DateTime.Now.AddMinutes(_settings.ExpiresInMin),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static List<Claim> CreateInitialClaims(string businessGroup, Guid bereavementReferenceNumber)
    {
        return new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, businessGroup + bereavementReferenceNumber),
                new Claim(ClaimTypes.Role, "BereavementInitialUser"),
                new Claim("bereavement_reference_number", bereavementReferenceNumber.ToString()),
                new Claim("business_group", businessGroup),
            };
    }
}