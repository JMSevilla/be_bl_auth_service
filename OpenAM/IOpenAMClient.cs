using LanguageExt;

namespace WTW.AuthenticationService.OpenAM;

public interface IOpenAMClient
{
    Task<Either<ErrorResponse, SessionResponse>> Session(
        string tokenId);

    Task<Either<ErrorResponse, SessionPropertiesResponse>> SessionProperties(string cookieName,
        string tokenId);

    Task<Either<ErrorResponse, FinalAuthenticationResponse>> Authenticate(
        string username,
        string password,
        string realm);

    Task<Either<ErrorResponse, Unit>> Logout(string token);
    Task<Either<ErrorResponse, ValidationResponse>> ValidateSession(string token);
}