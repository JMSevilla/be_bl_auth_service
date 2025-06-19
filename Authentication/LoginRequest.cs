using System.ComponentModel.DataAnnotations;
using WTW.Web.Validation;

namespace WTW.AuthenticationService.Authentication;
public record LoginRequest
{
    [Required]
    public string UserName { get; init; }

    [Required]
    public string Password { get; init; }

    [Required]
    public string Realm { get; init; }

    [RequiredList]
    public IReadOnlyCollection<string> BusinessGroups { get; init; } = new List<string>();
}