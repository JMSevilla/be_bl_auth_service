using System.ComponentModel.DataAnnotations;

namespace WTW.AuthenticationService.Tokens;

public record RefreshTokenRequest
{
    [Required]
    public string AccessToken { get; init; }

    [Required]
    public string RefreshToken { get; init; }
}