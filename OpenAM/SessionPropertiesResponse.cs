using System.Text.Json.Serialization;

namespace WTW.AuthenticationService.OpenAM;

public record SessionPropertiesResponse
{
    [JsonPropertyName("AMCtxId")]
    public string AMCtxId { get; set; }

    [JsonPropertyName("epa_userid")]
    public string EpaUserid { get; set; }

    [JsonPropertyName("epa_refno")]
    public string EpaRefno { get; set; }

    [JsonPropertyName("epa_bgroup")]
    public string EpaBgroup { get; set; }
}