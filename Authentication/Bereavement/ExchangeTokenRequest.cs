using System.ComponentModel.DataAnnotations;

namespace WTW.AuthenticationService.Authentication.Bereavement;

public record ExchangeTokenRequest
{
    [Required]
    public string AccessToken { get; init; }

    [Required]
    public string RefreshToken { get; init; }
}