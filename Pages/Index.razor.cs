using Microsoft.JSInterop;
using platejury_app.Data;

namespace platejury_app.Pages;

public partial class Index
{
    private bool _isLoaded = false;
    private Playlist? Playlist;
    private List<Item> SelectedTracks = [];
    private string? SelectedVoterId;
    private Dictionary<string, string> DisplayNames = [];
    private Dictionary<string, string> TrackNames = [];
    private Dictionary<string,DateTime> DuplicateTracks = [];
    private Dictionary<string,bool> VoteButtonStates = [];
    private List<Votes> Votes = [];
    private IEnumerable<ResultRow> Results = [];

    protected override void OnInitialized()
    {
        if (_isLoaded == false )
        {
            _isLoaded = true;
            _ = LoadDataAsync();
        }
    }
    private async Task LoadDataAsync()
    {
        // get playlist from spotify api
        Playlist = await playlistService.GetPlaylistAsync();
        // get users who have added songs on the playlist, and get their display names from spotify api
        List<string>? users = Playlist?.Tracks.Items.Select(x => x.AddedBy.Id).ToList();
        if (users != null)
            DisplayNames = await playlistService.GetUserDisplayNames(users);
        // get display names for tracks
        TrackNames = GetTrackDisplayNames();
        DuplicateTracks = await historyService.FindDuplicateTracks([.. TrackNames.Keys]);
        // get votes from db and calculate results
        await RefreshSubmittedVotes();
        StateHasChanged(); // Ensures UI updates when data is ready
    }

    private string GetWallOfShame()
    {
        var ret = string.Empty;
        if(Playlist == null)
            return ret;

        foreach (var item in Playlist.Tracks.Items.Where(x => !Votes.Any(y => y.VoterId == x.AddedBy.Id)))
        {
            if (string.IsNullOrEmpty(ret) == false)
            {
                ret += ", ";
            }
            if (DisplayNames.TryGetValue(item.AddedBy.Id, out string? name))
            {
                ret += name;
            }
            else
            {
                ret += item.AddedBy.Id;
            }
        }
        return ret;
    }

    private Dictionary<string, string> GetTrackDisplayNames()
    {
        Dictionary<string, string> displayNames = [];

        if (Playlist == null)
        {
            return displayNames;
        }

        var tracks = Playlist.Tracks.Items.Select(x => new { x.Track.Id, x.Track.Name, ArtistName = x.Track.Artists[0].Name });
        foreach (var track in tracks)
        {
            displayNames[track.Id] = $"{track.Name} by {track.ArtistName}";
        }
        return displayNames;
    }

    private async Task RefreshSubmittedVotes()
    {
        Votes = await votingService.GetVotes();
        Results = votingService.GetResults(Votes).OrderByDescending(_ => _);
    }

    // Add tracks to or remove from voting list
    private void Vote(Item item)
    {
        VoteButtonStates[item.Track.Id] = !VoteButtonStates[item.Track.Id];
        if (SelectedTracks.Remove(item) == false)
        {
            SelectedTracks.Add(item);
        }
    }

    // Save the current top 5 from voting list to db
    private async Task SubmitVotes()
    {
        if (string.IsNullOrEmpty(SelectedVoterId))
        {
            await JsRuntime.InvokeVoidAsync("alert", "Voter not selected!");

        }
        else if (SelectedTracks.Count > 0 || await JsRuntime.InvokeAsync<bool>("confirm", "Are you sure you want to vote empty?"))
        {
            var trackSelection = SelectedTracks.Count > 5 ? SelectedTracks.GetRange(0, 5) : SelectedTracks;
            var ret = await votingService.AddVotes(SelectedVoterId, trackSelection);
            if (ret.IsSuccess)
            {
                await RefreshSubmittedVotes();
                //Clear form
                SelectedTracks.Clear();
                SelectedVoterId = null;
                foreach(var k in VoteButtonStates.Keys)
                {
                    VoteButtonStates[k] = false;
                }
                await JsRuntime.InvokeVoidAsync("alert", "Your votes have been registered, thank you for your service!");
            }
            else
            {
                await JsRuntime.InvokeVoidAsync("alert", ret.Message);
            }
        }
    }
}