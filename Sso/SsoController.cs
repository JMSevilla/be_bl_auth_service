using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WTW.AuthenticationService.Authentication;
using WTW.AuthenticationService.Domain;
using WTW.AuthenticationService.Infrastructure;
using WTW.AuthenticationService.Infrastructure.AccessTokens;
using WTW.AuthenticationService.Infrastructure.AuthenticationDb;
using WTW.AuthenticationService.OpenAM;
using WTW.AuthenticationService.Tokens;
using WTW.Web.Errors;
using WTW.Web.LanguageExt;
using WTW.Web.Logging;

namespace WTW.AuthenticationService.Sso
{
    [ApiController]
    [Route("api/sso")]
    public class SsoController : ControllerBase
    {
        private readonly IOpenAMClient _openAm;
        private readonly IMdpClient _mdpClient;
        private readonly IUserRepository _userRepository;
        private readonly IAccessToken _accessToken;
        private readonly IRefreshTokenFactory _refreshTokenFactory;
        private readonly IAuthenticationDbUow _uow;
        private readonly ILogger<SsoController> _logger;

        public SsoController(
            IOpenAMClient openAm,
            IMdpClient mdpClient,
            IUserRepository userRepository,
            IAccessToken accessToken,
            IRefreshTokenFactory refreshTokenFactory,
            IAuthenticationDbUow uow,
            ILogger<SsoController> logger)
        {
            _openAm = openAm;
            _mdpClient = mdpClient;
            _userRepository = userRepository;
            _accessToken = accessToken;
            _refreshTokenFactory = refreshTokenFactory;
            _uow = uow;
            _logger = logger;
        }

        [HttpGet("session")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ApiError), 401)]
        public async Task<IActionResult> Session()
        {
            return (await _openAm.Session(HttpContext.User.FindFirst("token_id")?.Value))
                .Match<IActionResult>(_ =>
                {
                    return NoContent();
                },
                l =>
                {
                    _logger.LogInformationObject(l, "Failed to get session. OpenAM response:");
                    return Unauthorized(ApiError.From(OpenAMErrors.From(l.Message), "ERROR_SESSION_EXPIRED"));
                });
        }

        [AllowAnonymous]
        [HttpPost("session/create")]
        [ProducesResponseType(typeof(SsoSessionResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 400)]
        [ProducesResponseType(typeof(ApiError), 401)]
        public async Task<IActionResult> CreateSession(SsoSessionRequest request)
        {
            return await (await _openAm.Session(request.TokenId))
                .MatchAsync<IActionResult>(async r =>
                {
                    _logger.LogInformationObject(r, "OpenAM response:");
                    if (r.Realm != request.Realm)
                        return BadRequest(new ApiError("AUTH_FORM_INVALID_USER_ACCOUNT_REALM_ERROR"));

                    var userResult = await _userRepository.Find(r.Username);
                    if (userResult.IsNone)
                        return BadRequest(new ApiError("AUTH_FORM_INVALID_USER_ACCOUNT_ERROR"));
                    
                    var user = userResult.Value();
                    var propertiesResult = await _openAm.SessionProperties(request.CookieName, request.TokenId);
                    if (propertiesResult.IsLeft)
                        return BadRequest(ApiError.FromMessage(propertiesResult.Left().Message));

                    string accessToken;
                    var properties = propertiesResult.Right();
                    _logger.LogInformationObject(properties, "OpenAM session properties:");
                    var epaUser = new EpaUser(properties.EpaRefno, properties.EpaBgroup);
                    if (epaUser.IsValid())
                    {
                        if (epaUser.IsValidForUser(user))
                        {
                            accessToken = _accessToken.Create(
                                user.UserName,
                                epaUser.BusinessGroup,
                                epaUser.ReferenceNumber,
                                user.BusinessGroup,
                                user.ReferenceNumber,
                                Guid.NewGuid(),
                                request.TokenId);
                            var response = await _mdpClient.FindLinkedMembers(accessToken, user.BusinessGroup);
                            if (!response.LinkedMembers.Any())
                                return BadRequest(ApiError.FromMessage("Linked user doesn't belong to the current user"));
                        }
                        else
                        {
                            accessToken = _accessToken.Create(
                                user.UserName,
                                user.BusinessGroup ?? epaUser.BusinessGroup,
                                user.ReferenceNumber ?? epaUser.ReferenceNumber,
                                Guid.NewGuid(),
                                request.TokenId);
                        }
                    }
                    else
                    {
                        accessToken = _accessToken.Create(
                            user.UserName,
                            user.BusinessGroup,
                            user.ReferenceNumber,
                            Guid.NewGuid(),
                            request.TokenId);
                    }
                    
                    var refreshToken = _refreshTokenFactory.Create(
                        userResult.Value().UserName,
                        request.TokenId,
                        DateTimeOffset.UtcNow);

                    _uow.Add(refreshToken);
                    await _uow.Commit();

                    return Ok(new SsoSessionResponse(accessToken, refreshToken.Value));
                },
                l =>
                {
                    _logger.LogInformationObject(l, "Failed to create session. OpenAM response:");
                    return Unauthorized(new ApiError(OpenAMErrors.From(l.Message)));
                });
        }

        [HttpPost("session/keep-alive")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ApiError), 401)]
        public async Task<IActionResult> KeepAlive()
        {
            var tokenId = HttpContext.User.FindFirst("token_id").Value;
            return await (await _openAm.ValidateSession(tokenId))
                .MatchAsync<IActionResult>(async r =>
                {
                    return r.Valid ? NoContent() : Unauthorized(ApiError.From("Unauthorized", "ERROR_SESSION_EXPIRED"));
                },
                l =>
                {
                    _logger.LogInformationObject(l, "Failed to keep session alive. OpenAM response:");
                    return Unauthorized(new ApiError(OpenAMErrors.From(l.Message)));
                });
        }
    }
}