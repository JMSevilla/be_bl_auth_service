using System.Text.Json.Serialization;

namespace WTW.AuthenticationService.OpenAM;
public record ErrorResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("reason")]
    public string Reason { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }
}