using System.Security.Cryptography;
using WTW.AuthenticationService.Domain;
using WTW.Web.Authentication;

namespace WTW.AuthenticationService.Tokens;

public class RefreshTokenFactory : IRefreshTokenFactory
{
    private readonly AuthenticationSettings _settings;

    public RefreshTokenFactory(AuthenticationSettings settings)
    {
        _settings = settings;
    }

    public RefreshToken Create(
        string userId,
        string sessionId,
        DateTimeOffset now)
    {
        var randomNumber = GenerateRandomNumber();

        return new RefreshToken(
            userId,
            sessionId,
            Convert.ToBase64String(randomNumber),
            now.AddMinutes(_settings.RefreshExpiresInMin));
    }

    private byte[] GenerateRandomNumber()
    {
        var randomNumber = new byte[64];
        using var generator = RandomNumberGenerator.Create();
        generator.GetBytes(randomNumber);
        return randomNumber;
    }
}