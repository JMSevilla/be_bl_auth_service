using WTW.AuthenticationService.Domain;

namespace WTW.AuthenticationService.Infrastructure;

public class LinkedMembersResponse
{
    public IReadOnlyList<EpaUser> LinkedMembers { get; set; }
}