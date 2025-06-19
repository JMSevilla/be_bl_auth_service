using System.ComponentModel.DataAnnotations;
using WTW.Web.Validation;

namespace WTW.AuthenticationService.Authentication.Bereavement;
public record BereavementLoginRequest
{
    [Required]
    public string BusinessGroup { get; init; }
}