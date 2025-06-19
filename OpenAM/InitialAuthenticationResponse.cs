using System.Text.Json.Serialization;

namespace WTW.AuthenticationService.OpenAM;
public record InitialAuthenticationResponse
{
    [JsonPropertyName("tokenId")]
    public string TokenId { get; set; }

    [JsonPropertyName("authId")]
    public string AuthId { get; set; }

    [JsonPropertyName("template")]
    public string Template { get; set; }

    [JsonPropertyName("stage")]
    public string Stage { get; set; }

    [JsonPropertyName("header")]
    public string Header { get; set; }

    [JsonPropertyName("callbacks")]
    public List<Callback> Callbacks { get; set; } = new();
}

public record Callback
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("output")]
    public List<Output> Output { get; set; } = new();

    [JsonPropertyName("input")]
    public List<Input> Input { get; set; } = new();
}

public record Output
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }
}

public record Input
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }
}