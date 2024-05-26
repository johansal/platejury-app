using System.Net.Http.Headers;
using System.Text.Json;

namespace platejury_app.Data;

public class PlaylistService(ILogger<PlaylistService> logger, string clientId, string clientSecret, string tokenUri, string playlistUri, string playlistId)
{
    private readonly HttpClient client = new();
    private readonly ILogger<PlaylistService> logger = logger;
    private AccessToken? accessToken;
    public async Task<Playlist?> GetPlaylistAsync()
    {
        if(accessToken == null || accessToken.IsExpired())
        {
            accessToken = await GetAccessTokenAsync();
            if(accessToken == null) 
            {
                return null;
            }
        }

        using var requestMessage = 
            new HttpRequestMessage(
                HttpMethod.Get, 
                $"{playlistUri}/{playlistId}"
            );
        requestMessage.Headers.Authorization =
            new AuthenticationHeaderValue(accessToken.Type, accessToken.Token);

        var response = await client.SendAsync(requestMessage);
        using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
        var responseStr = await reader.ReadToEndAsync();
        
        if (response.IsSuccessStatusCode == false)
        {
            logger.LogError("Couldn't get playlist details: {error}",responseStr);
            return null;
        }
        var playlist = JsonSerializer.Deserialize<Playlist>(responseStr);
        // Check playlist for multiple items from same user and disqualify them.
        if(playlist != null) 
        {
            playlist.Tracks.Items = playlist.Tracks.Items.GroupBy(x => x.AddedBy.Id)
                .Where(x => x.Count() == 1)
                .Select(x => x.First()).ToList();  
        }
        return playlist;
    }
    public async Task<Dictionary<string, string>> GetUserDisplayNames(List<string> userIds) 
    {
        var displayNames = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText("displaynames.json"));
        if(displayNames == null) {
            logger.LogError("Parsing display names from file failed!");
            displayNames = [];
        }

        foreach(var user in userIds) {
            if(displayNames.ContainsKey(user) == false)
            {
                logger.LogWarning("Unkonown user {id}, getting displayname from API", user);
                if(accessToken == null || accessToken.IsExpired())
                {
                    accessToken = await GetAccessTokenAsync();
                    if(accessToken == null) 
                    {
                        return displayNames;
                    }
                }
                using var requestMessage = 
                    new HttpRequestMessage(
                        HttpMethod.Get, 
                        $"https://api.spotify.com/v1/users/{user}"
                    );
                requestMessage.Headers.Authorization =
                    new AuthenticationHeaderValue(accessToken.Type, accessToken.Token);

                var response = await client.SendAsync(requestMessage);
                using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
                var responseStr = await reader.ReadToEndAsync();
                
                if (response.IsSuccessStatusCode == false)
                {
                    logger.LogError("Couldn't get user details for {user}: {error}", user, responseStr);
                }
                else {
                    var displayName = JsonDocument.Parse(responseStr).RootElement.GetProperty("display_name").GetString();
                    if(displayName != null)
                        displayNames[user] = displayName;
                    else {
                        logger.LogWarning("Failed to parse display_name for {user}", user);
                    }
                }       
            }
        }
        return displayNames;
    }
    private async Task<AccessToken?> GetAccessTokenAsync()
    {
        var response = await client.PostAsync(
            new Uri(tokenUri),
            new FormUrlEncodedContent([
                new KeyValuePair<string, string>(
                    "grant_type", 
                    "client_credentials"),
                new KeyValuePair<string, string>(
                    "client_id", 
                    clientId),
                new KeyValuePair<string, string>(
                    "client_secret", 
                    clientSecret),
            ])
        );

        // Get the response content.
        HttpContent responseContent = response.Content;

        // Get the stream of the content.
        using var reader = new StreamReader(await responseContent.ReadAsStreamAsync());
        // Write the output.
        var responseStr = await reader.ReadToEndAsync();

        if (response.IsSuccessStatusCode == false)
        {
            logger.LogError("Couldn't get access token: {error}",responseStr);
            return null;
        }
        else
        {
            return JsonSerializer.Deserialize<AccessToken>(responseStr);
        }
    }
}
