using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using LanguageExt;
using WTW.Web.Clients;

namespace WTW.AuthenticationService.OpenAM
{
    public class OpenAMClient : IOpenAMClient
    {
        private readonly HttpClient _client;

        public OpenAMClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<Either<ErrorResponse, SessionResponse>> Session(
            string tokenId)
        {
            return await _client.PostJson<SessionResponse, ErrorResponse>(
                $"/twacm/json/sessions?tokenId={tokenId}&_action=getSessionInfo",
                ("Accept-API-Version", "resource=2.1"));
        }
        
        public async Task<Either<ErrorResponse, SessionPropertiesResponse>> SessionProperties(string cookieName,
            string tokenId)
        {
            return await _client.PostJson<SessionPropertiesResponse, ErrorResponse>(
                $"/twacm/json/sessions?_action=getSessionProperties",
                ("Accept-API-Version", "resource=3.1"), (cookieName, tokenId));
        }

        public async Task<Either<ErrorResponse, FinalAuthenticationResponse>> Authenticate(
            string username,
            string password,
            string realm)
        {
            return await await Init(
                $"/twacm/json/authenticate?realm={realm}",
                ("Accept-API-Version", "resource=2.0, protocol=1.0"))
                .Map(r => Execute(
                    r,
                    $"/twacm/json/authenticate?realm={realm}",
                    ("Accept-API-Version", "resource=2.0, protocol=1.0"))
                    .ToAsync());

            async Task<HttpResponseMessage> Init(string url, params (string, string)[] headers)
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                headers.ToList().ForEach(h => request.Headers.Add(h.Item1, h.Item2));
                return await _client.SendAsync(request);
            }

            async Task<Either<ErrorResponse, FinalAuthenticationResponse>> Execute(
                HttpResponseMessage setupResponse, string url, params (string, string)[] headers)
            {
                if (setupResponse.StatusCode == HttpStatusCode.Unauthorized ||
                    setupResponse.StatusCode == HttpStatusCode.BadRequest)
                    return await setupResponse.Content.ReadFromJsonAsync<ErrorResponse>();

                var authenticationBody = FillCredentials(
                    await setupResponse.Content.ReadFromJsonAsync<InitialAuthenticationResponse>(),
                    username,
                    password);

                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(
                        JsonSerializer.Serialize(authenticationBody),
                        Encoding.UTF8,
                        "application/json")
                };
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (setupResponse.Headers.TryGetValues("Set-Cookie", out IEnumerable<string> cookieValues))
                    request.Headers.TryAddWithoutValidation("Cookie", cookieValues);
                headers.ToList().ForEach(h => request.Headers.Add(h.Item1, h.Item2));

                var finalResponse = await _client.SendAsync(request);
                if (finalResponse.StatusCode == HttpStatusCode.Unauthorized ||
                    finalResponse.StatusCode == HttpStatusCode.BadRequest)
                    return await finalResponse.Content.ReadFromJsonAsync<ErrorResponse>();

                return await finalResponse.Content.ReadFromJsonAsync<FinalAuthenticationResponse>();
            }
        }

        public async Task<Either<ErrorResponse, Unit>> Logout(string token)
        {
            return await _client.PostJson<Unit, ErrorResponse>(
                $"/twacm/json/sessions?tokenId={token}&_action=logout",
                ("Cookie", $"epav2_syst={token}"),
                ("Accept-API-Version", "resource=2.1"));
        }

        public async Task<Either<ErrorResponse, ValidationResponse>> ValidateSession(string token)
        {
            return await _client.PostJson<ValidationResponse, ErrorResponse>(
                $"/twacm/json/sessions?tokenId={token}&_action=validate",
                ("Accept-API-Version", "resource=1.2"));
        }

        private static InitialAuthenticationResponse FillCredentials(InitialAuthenticationResponse response,
            string username, string password)
        {
            response.Callbacks
                .Single(x => x.Type == "NameCallback" && x.Input[0].Name == "IDToken1")
                .Input[0]
                .Value = username;

            response.Callbacks
                .Single(x => x.Type == "PasswordCallback" && x.Input[0].Name == "IDToken2")
                .Input[0]
                .Value = password;

            return response;
        }
    }
}