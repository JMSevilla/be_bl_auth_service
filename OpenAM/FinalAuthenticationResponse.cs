using System.Text.Json.Serialization;

namespace WTW.AuthenticationService.OpenAM;
public record FinalAuthenticationResponse
{
    [JsonPropertyName("tokenId")]
    public string TokenId { get; set; }

    [JsonPropertyName("successUrl")]
    public string SuccessUrl { get; set; }

    [JsonPropertyName("realm")]
    public string Realm { get; set; }
}