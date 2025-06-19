using System.ComponentModel.DataAnnotations;
using WTW.Web.Validation;

namespace WTW.AuthenticationService.Authentication;
public record LogoutRequest
{
    [Required]
    public string AccessToken { get; init; }

    [Required]
    public string RefreshToken { get; init; }
}