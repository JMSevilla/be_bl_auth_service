namespace WTW.AuthenticationService.Authentication
{
    public record LoginResponse(string AccessToken, string RefreshToken);
}