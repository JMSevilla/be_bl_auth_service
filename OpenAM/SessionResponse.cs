using System.Text.Json.Serialization;

namespace WTW.AuthenticationService.OpenAM;
public record SessionResponse
{
    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("universalId")]
    public string UniversalId { get; set; }

    [JsonPropertyName("realm")]
    public string Realm { get; set; }

    [JsonPropertyName("latestAccessTime")]
    public DateTimeOffset? LatestAccessTime { get; set; }

    [JsonPropertyName("maxIdleExpirationTime")]
    public DateTimeOffset? MaxIdleExpirationTime { get; set; }

    [JsonPropertyName("maxSessionExpirationTime")]
    public DateTimeOffset? MaxSessionExpirationTime { get; set; }
}