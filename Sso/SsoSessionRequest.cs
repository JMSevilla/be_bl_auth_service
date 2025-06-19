using System.ComponentModel.DataAnnotations;

namespace WTW.AuthenticationService.Authentication
{
#warning Remove hardcoded realm when preprod is updated
    public record SsoSessionRequest([Required] string TokenId, [Required] string CookieName, string Realm = "/mdp/natwest");
}
