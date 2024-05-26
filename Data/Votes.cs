namespace platejury_app.Data;

public class Votes {
    public required string Id {get; set;}
    public required string VoterId {get; set;}
    public required List<Dictionary<string, string>> VotedTracks{get; set;}
    public DateTime ResultDay {get; set;}
}