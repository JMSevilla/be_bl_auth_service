using LanguageExt;

namespace WTW.AuthenticationService.Infrastructure
{
    public static class OpenAMErrors
    {
        private static readonly Map<string, string> _errors = Map.create(
            ("AUTHENTICATION ERROR", "AUTH_FORM_AUTHENTICATION_ERROR"),
            ("USERID/PASSWORD INVALID", "AUTH_FORM_INVALID_CREDENTIALS_ERROR"),
            ("IPADDRESS CHECK FAILED", "AUTH_FORM_INVALID_IP_ADDRESS_ERROR"),
            ("AUTHENTICATION METHOD NOT SUPPORTED", "AUTH_FORM_INVALID_METHOD_ERROR"),
            ("ACCOUNT LOCKED", "AUTH_FORM_ACCOUNT_LOCKED_ERROR"),
            ("NO EPA PORTAL ACCESS", "AUTH_FORM_NO_ACCESS_ERROR"));

        public static string From(string message)
        {
            return _errors.Find(message).IfNone("AUTH_FORM_UNKNOWN_ERROR");
        }
    }
}