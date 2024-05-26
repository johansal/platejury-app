using System.Text.Json.Serialization;

namespace platejury_app.Data;
public class TrackList
{
    [JsonPropertyName("tracks")]
    public required List<TrackListTrack> TrackListTracks {get; set;}
}
public class TrackListTrack {
    [JsonPropertyName("artists")]
    public required List<Artist> Artists {get; set;}
    [JsonPropertyName("name")]
    public required string Name { get; set; }

}
