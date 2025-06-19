using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using WTW.AuthenticationService.Infrastructure.AccessTokens;
using WTW.AuthenticationService.Infrastructure.AuthenticationDb;
using WTW.AuthenticationService.Tokens;
using WTW.Web.Authorization;
using WTW.Web.Errors;

namespace WTW.AuthenticationService.Authentication.Bereavement;

[ApiController]
[Route("api/bereavement/")]
public class BereavementAuthenticationController : ControllerBase
{
    private readonly IBereavementToken _bereavementToken;
    private readonly IRefreshTokenFactory _refreshTokenFactory;
    private readonly IAuthenticationDbUow _uow;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BereavementAuthenticationController> _logger;

    public BereavementAuthenticationController(
        IBereavementToken bereavementToken,
        IRefreshTokenFactory refreshTokenFactory,
        IAuthenticationDbUow uow,
        IConfiguration configuration,
        ILogger<BereavementAuthenticationController> logger)
    {
        _bereavementToken = bereavementToken;
        _refreshTokenFactory = refreshTokenFactory;
        _uow = uow;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] BereavementLoginRequest request)
    {
        var customAuthorizationHeader = HttpContext.Request.Headers["X-Custom-Authorization"].FirstOrDefault();
        var serviceUuidJson = _configuration["SERVICE_UUID"];
        using var serviceUuidDoc = JsonDocument.Parse(serviceUuidJson);
        var bereavementUuid = serviceUuidDoc.RootElement.GetProperty("bereavement").GetString();
        var expectedHeaderValue = $"Basic {Convert.ToBase64String(Encoding.ASCII.GetBytes($"{bereavementUuid}"))}";
        if (customAuthorizationHeader != expectedHeaderValue)
        {
            _logger.LogError("Bereavement SERVICE_UUID does not match");
            return Unauthorized(ApiError.Unauthorized());
        }            

        var sessionId = Guid.NewGuid();
        var accessToken = _bereavementToken.CreateInitial(request.BusinessGroup, sessionId);
        var refreshToken = _refreshTokenFactory.Create(request.BusinessGroup, sessionId.ToString(), DateTimeOffset.UtcNow);

        _uow.Add(refreshToken);
        await _uow.Commit();

        return Ok(new LoginResponse(accessToken, refreshToken.Value));
    }

    [HttpPost("email/verified/exchange-token")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [Authorize(Policy = "BereavementInitialUser")]
    public async Task<IActionResult> EmailVerifiedToken([FromBody] ExchangeTokenRequest request)
    {
        if (_bereavementToken.Validate(request.AccessToken).Try().IsFaulted ||
            Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", String.Empty) != request.AccessToken)
            return Unauthorized(ApiError.FromMessage("Invalid access token"));

        var dbRefreshToken = (await _uow.Tokens.Find(request.RefreshToken)).SingleOrDefault();
        if (dbRefreshToken == null)
            return Unauthorized(ApiError.Unauthorized());

        if (!dbRefreshToken.IsActive(DateTimeOffset.UtcNow))
        {
            _uow.Remove(dbRefreshToken);
            await _uow.Commit();
            return Unauthorized(ApiError.FromMessage("Refresh token is expired"));
        }

        var (bereavementReferenceNumber, _) = HttpContext.User.BereavementUser();

        var accessToken = _bereavementToken.CreateWithEmailVerifiedRole(dbRefreshToken.UserId, bereavementReferenceNumber);

        var refreshToken = _refreshTokenFactory.Create(dbRefreshToken.UserId, dbRefreshToken.SessionId, DateTimeOffset.UtcNow);

        _uow.Remove(dbRefreshToken);
        _uow.Add(refreshToken);
        await _uow.Commit();

        return Ok(new RefreshTokenResponse(accessToken, refreshToken.Value));
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(RefreshTokenResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 401)]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (_bereavementToken.Validate(request.AccessToken).Try().IsFaulted)
            return Unauthorized(ApiError.FromMessage("Invalid access token"));

        var dbRefreshToken = (await _uow.Tokens.Find(request.RefreshToken)).SingleOrDefault();
        if (dbRefreshToken == null)
            return Unauthorized(ApiError.Unauthorized());

        if (!dbRefreshToken.IsActive(DateTimeOffset.UtcNow))
        {
            _uow.Remove(dbRefreshToken);
            await _uow.Commit();
            return Unauthorized(ApiError.FromMessage("Refresh token is expired"));
        }

        if (!Guid.TryParse(dbRefreshToken.SessionId, out var bereavementReferenceNumber))
            return Unauthorized(ApiError.FromMessage("Invalid access token"));

        string accessToken;
        if (request.AccessToken.ContainsRole("BereavementEmailVerifiedUser"))
            accessToken = _bereavementToken.CreateWithEmailVerifiedRole(dbRefreshToken.UserId, bereavementReferenceNumber);
        else
            accessToken = _bereavementToken.CreateInitial(dbRefreshToken.UserId, bereavementReferenceNumber);

        var refreshToken = _refreshTokenFactory.Create(dbRefreshToken.UserId, dbRefreshToken.SessionId, DateTimeOffset.UtcNow);

        _uow.Remove(dbRefreshToken);
        _uow.Add(refreshToken);
        await _uow.Commit();

        return Ok(new RefreshTokenResponse(accessToken, refreshToken.Value));
    }
}
