namespace WTW.AuthenticationService.Authentication
{
    public record SsoSessionResponse(string AccessToken, string RefreshToken);
}