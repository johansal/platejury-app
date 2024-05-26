using System.Text.Json.Serialization;

namespace platejury_app.Data;

public class AccessToken {
    [JsonPropertyName("access_token")]
    public required string Token {get; set;}
    [JsonPropertyName("token_type")]
    public required string Type {get; set;}
    [JsonPropertyName("expires_in")]
    public int LifespanSeconds {get; set;}
    private readonly DateTime Created = DateTime.Now;
    public bool IsExpired()
    {
        var currentTime = DateTime.Now;
        return currentTime.Subtract(Created).TotalSeconds > LifespanSeconds;
    }
}
