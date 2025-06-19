using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WTW.AuthenticationService.Domain;
using WTW.AuthenticationService.Infrastructure;
using WTW.AuthenticationService.Infrastructure.AccessTokens;
using WTW.AuthenticationService.Infrastructure.AuthenticationDb;
using WTW.Web.Authorization;
using WTW.Web.Errors;

namespace WTW.AuthenticationService.Tokens
{
    [ApiController]
    [Route("api/authentication")]
    public class RefreshTokensController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IAccessToken _accessToken;
        private readonly IAuthenticationDbUow _uow;
        private readonly IRefreshTokenFactory _refreshTokenFactory;
        private readonly ILogger<RefreshTokensController> _logger;

        public RefreshTokensController(
            IUserRepository userRepository,
            IAccessToken accessToken,
            IRefreshTokenFactory refreshTokenFactory,
            IAuthenticationDbUow uow,
            ILogger<RefreshTokensController> logger)
        {
            _userRepository = userRepository;
            _accessToken = accessToken;
            _uow = uow;
            _refreshTokenFactory = refreshTokenFactory;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(RefreshTokenResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 400)]
        [ProducesResponseType(typeof(ApiError), 401)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (!_accessToken.Validate(request.AccessToken).Try().IsSuccess)
                return Unauthorized(ApiError.FromMessage("Invalid access token"));

            RefreshToken dbRefreshToken = null;
            var result = await _uow.Tokens.Find(request.RefreshToken);
            if (result.IsSome)
            {
                dbRefreshToken = result.SingleOrDefault();
            }

            if (dbRefreshToken == null)
                return Unauthorized(ApiError.Unauthorized());

            if (!dbRefreshToken.IsActive(DateTimeOffset.UtcNow))
            {
                _uow.Remove(dbRefreshToken);
                await _uow.Commit();
                return Unauthorized(ApiError.FromMessage("Refresh token is expired"));
            }

            User user = null;
            var userResult = await _userRepository.Find(dbRefreshToken.UserId);
            if (userResult.IsSome)
            {
                user = userResult.SingleOrDefault();
            }

            if (user == null)
                return Unauthorized(ApiError.FromMessage("User not found"));

            var accessToken = _accessToken.Create(
                user.UserName,
                user.BusinessGroup,
                user.ReferenceNumber ?? string.Empty,
                Guid.Parse(request.AccessToken.GetClaimValues("bereavement_reference_number").Single()),
                dbRefreshToken.SessionId);

            var refreshToken = _refreshTokenFactory.Create(
                user.UserName,
                dbRefreshToken.SessionId,
                DateTimeOffset.UtcNow);

            try
            {
                _uow.Remove(dbRefreshToken);
                _uow.Add(refreshToken);
                await _uow.Commit();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "DbUpdateConcurrencyException occured. Refno: {refNo}, BGroup: {bgroup}, SessionId: {sessionId}", user.ReferenceNumber, user.BusinessGroup, dbRefreshToken.SessionId);
                return NotFound(ApiError.FromMessage("Current Refresh token not found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepion occured during refresh token update. Refno: {refNo}, BGroup: {bgroup}", user.ReferenceNumber, user.BusinessGroup);
                return Unauthorized(ApiError.FromMessage("Refresh token failed to update"));
            }

            return Ok(new RefreshTokenResponse(accessToken, refreshToken.Value));
        }
    }
}