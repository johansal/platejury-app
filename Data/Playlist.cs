using System.Text.Json.Serialization;

namespace platejury_app.Data;
public class Playlist
{
    [JsonPropertyName("description")]
    public required string Description {get; set;}
    [JsonPropertyName("images")]
    public required List<Image> Images {get; set;}
    [JsonPropertyName("tracks")]
    public required Tracks Tracks {get; set;}
}
public class Tracks
{
    [JsonPropertyName("items")]
    public required List<Item> Items {get; set;}
}
public class Item 
{
    [JsonPropertyName("added_by")]
    public required User AddedBy {get; set;}
    [JsonPropertyName("track")]
    public required Track Track {get; set;}
}
public class Track
{
    [JsonPropertyName("id")]
    public required string Id {get; set;}
    [JsonPropertyName("album")]
    public required Album Album {get; set;}
    [JsonPropertyName("artists")]
    public required List<Artist> Artists {get; set;}
    [JsonPropertyName("name")]
    public required string Name {get; set;}
    [JsonPropertyName("external_urls")]
    public required ExternalUrls ExternalUrls {get; set;}
}
public class ExternalUrls
{
    [JsonPropertyName("spotify")]
    public required string Spotify {get; set;}
}
public class Album
{
    [JsonPropertyName("images")]
    public required List<Image> Images {get; set;}
}
public class Artist
{
    [JsonPropertyName("name")]
    public required string Name { get; set;}
}
public class User
{
    [JsonPropertyName("id")]
    public required string Id {get; set;}
    [JsonPropertyName("href")]
    public required string Href {get; set;}
}
public class Image
{
    [JsonPropertyName("url")]
    public required string Url {get; set;}
}