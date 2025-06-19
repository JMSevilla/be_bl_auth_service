namespace WTW.AuthenticationService.Tokens
{
    public record RefreshTokenResponse(string AccessToken, string RefreshToken);
}