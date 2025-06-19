using WTW.AuthenticationService.Domain;

namespace WTW.AuthenticationService.Infrastructure;

public interface IMdpClient
{
    Task<LinkedMembersResponse> FindLinkedMembers(string token, string bgroup);
}