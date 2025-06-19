using System.Text.Json;
using WTW.AuthenticationService.Domain;
using WTW.Web.Clients;

namespace WTW.AuthenticationService.Infrastructure;

public class MdpClient : IMdpClient
{
    private readonly HttpClient _client;

    public MdpClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<LinkedMembersResponse> FindLinkedMembers(string token, string bgroup)
    {
        return await _client.GetJson<LinkedMembersResponse>($"/mdp-api/api/members/linked-members", ("Authorization", "Bearer " + token), ("bgroup", bgroup));
    }
}