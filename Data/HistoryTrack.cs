namespace platejury_app.Data;
public class HistoryTrack
{
    public required string Id {get; set;}
    public required string UserId {get; set;}
    public required string UserName {get; set;}
    public required string TrackId {get; set;}
    public required string TrackName {get; set;}
    public int Points  {get; set;}
    public int Position  {get; set;}
    public DateTime ResultDay {get; set;}

}