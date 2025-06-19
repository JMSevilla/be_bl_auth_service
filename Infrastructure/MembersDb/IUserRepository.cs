using LanguageExt;
using WTW.AuthenticationService.Domain;

namespace WTW.AuthenticationService.Infrastructure;

public interface IUserRepository
{
    Task<Option<User>> Find(string userName);
    Task<Option<User>> Find(string referenceNumber, string businessGroup);
}