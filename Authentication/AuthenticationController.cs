using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WTW.AuthenticationService.Infrastructure;
using WTW.AuthenticationService.Infrastructure.AccessTokens;
using WTW.AuthenticationService.Infrastructure.AuthenticationDb;
using WTW.AuthenticationService.OpenAM;
using WTW.AuthenticationService.Tokens;
using WTW.Web.Errors;
using WTW.Web.LanguageExt;
using WTW.Web.Logging;

namespace WTW.AuthenticationService.Authentication;

[ApiController]
[Route("api/authentication")]
public class AuthenticationController : ControllerBase
{
    private readonly IOpenAMClient _openAm;
    private readonly IUserRepository _userRepository;
    private readonly IAccessToken _accessToken;
    private readonly IRefreshTokenFactory _refreshTokenFactory;
    private readonly IAuthenticationDbUow _uow;
    private readonly ILogger<AuthenticationController> _logger;
    private readonly IHubContext<SessionHub> _hubContext;
    private readonly SessionManager _sessionManager;

    public AuthenticationController(
        IOpenAMClient openAm,
        IUserRepository userRepository,
        IAccessToken accessToken,
        IRefreshTokenFactory refreshTokenFactory,
        IAuthenticationDbUow uow,
        ILogger<AuthenticationController> logger,
        IHubContext<SessionHub> hubContext,
        SessionManager sessionManager)
    {
        _openAm = openAm;
        _userRepository = userRepository;
        _accessToken = accessToken;
        _refreshTokenFactory = refreshTokenFactory;
        _uow = uow;
        _logger = logger;
        _hubContext = hubContext;
        _sessionManager = sessionManager;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        return await (await _openAm.Authenticate(request.UserName, request.Password, request.Realm))
            .MatchAsync<IActionResult>(async r =>
            {
                if (r.Realm != request.Realm)
                    return BadRequest(new ApiError("AUTH_FORM_INVALID_USER_ACCOUNT_REALM_ERROR"));

                var user = await _userRepository.Find(request.UserName);
                if (!user.IsSome)
                    return BadRequest(new ApiError("AUTH_FORM_INVALID_USER_ACCOUNT_ERROR"));

                if (!user.Value().IsValidBusinessGroup(request.BusinessGroups))
                    return BadRequest(new ApiError("AUTH_FORM_INVALID_TENANT_ERROR"));

                if (_sessionManager.IsUserLoggedIn(request.UserName))
                    return BadRequest(new ApiError("Another session is active."));

                var accessToken = _accessToken.Create(
                    user.Value().UserName,
                    user.Value().BusinessGroup,
                    user.Value().ReferenceNumber ?? string.Empty,
                    Guid.NewGuid(),
                    r.TokenId);

                var refreshToken = _refreshTokenFactory.Create(
                    user.Value().UserName,
                    r.TokenId,
                    DateTimeOffset.UtcNow);
                _uow.Add(refreshToken);
                await _uow.Commit();
                return Ok(new LoginResponse(accessToken, refreshToken.Value));
            },
            l =>
            {
                _logger.LogInformationObject(l, "Failed to login. OpenAM response:");
                return BadRequest(new ApiError(OpenAMErrors.From(l.Message)));
            });
    }

    [HttpPost("logout")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 401)]
    [AllowAnonymous]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        var principal = _accessToken.Validate(request.AccessToken).Try();
        if (!principal.IsSuccess)
            return Unauthorized(ApiError.Unauthorized());

        var tokenId = principal.Value().FindFirst("token_id").Value;
        var response = await _openAm.Logout(tokenId);
        if (!response.IsRight)
            _logger.LogInformationObject(response.Left(), "Failed to logout. OpenAM response:");

        var token = (await _uow.Tokens.Find(request.RefreshToken)).SingleOrDefault();
        if (token != null)
        {
            try
            {
                _uow.Remove(token);
                await _uow.Commit();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "DbUpdateConcurrencyException occured during refresh token removal. Refresh token sessionId: {sessionId}", token.SessionId);
                return NotFound(ApiError.FromMessage("Current Refresh token not found"));
            }
        }

        return NoContent();
    }
}
