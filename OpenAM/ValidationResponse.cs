using System.Text.Json.Serialization;

namespace WTW.AuthenticationService.OpenAM
{
    public class ValidationResponse
    {
        [JsonPropertyName("valid")] public bool Valid { get; set; }

        [JsonPropertyName("sessionUid")] public string SessionUid { get; set; }

        [JsonPropertyName("uid")] public string Uid { get; set; }

        [JsonPropertyName("realm")] public string Realm { get; set; }
    }
}