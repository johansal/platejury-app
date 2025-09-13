using System.Runtime.InteropServices;
using Microsoft.JSInterop;
using platejury_app.Data;

namespace platejury_app.Pages;

public partial class Index
{
    private bool _isLoaded = false;
    private Playlist? Playlist;
    private List<Item> SelectedTracks = [];
    private string SelectedVoterId = string.Empty;
    private Dictionary<string, string> DisplayNames = [];
    private Dictionary<string, string> TrackNames = [];
    private Dictionary<string,DateTime> DuplicateTracks = [];
    private Dictionary<string,bool> VoteButtonStates = [];
    private List<Votes> Votes = [];
    private IEnumerable<ResultRow> Results = [];
    private (bool IsLoading, string ErrorText) SubmitVotesModal = (false, "");
    private Theme Theme = new();
    private Theme FormData = new();

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
        // get theme, default to playlist description in spotify if not set
        await RefreshTheme();
        // get votes from db and calculate results
        await RefreshSubmittedVotes();
        StateHasChanged(); // Ensures UI updates when data is ready
    }

    // Timezone aware check to see if its past 12 o clock in Helsinki on a voting day and someone hasnt voted
    private bool ShowWOS()
    {
        // windows and linux timezone id's are different
        string tzId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "E. Europe Standard Time"
            : "Europe/Helsinki";

        var tz = TimeZoneInfo.FindSystemTimeZoneById(tzId);
        var localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);

        return localTime.DayOfWeek == DayOfWeek.Thursday 
            && localTime.TimeOfDay.Hours >= 12
            && Votes.Count < Playlist?.Tracks.Items.Count;
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
        SubmitVotesModal.IsLoading = true;
        SubmitVotesModal.ErrorText = string.Empty;

        var trackSelection = SelectedTracks.Count > 5 ? SelectedTracks.GetRange(0, 5) : SelectedTracks;
        var ret = await votingService.AddVotes(SelectedVoterId, trackSelection);
        if (ret.IsSuccess)
        {
            await RefreshSubmittedVotes();
            //Clear form
            SelectedTracks.Clear();
            SelectedVoterId = string.Empty;
            foreach(var k in VoteButtonStates.Keys)
            {
                VoteButtonStates[k] = false;
            }
        }
        else
        {
            SubmitVotesModal.ErrorText = ret.Message ?? string.Empty;
        }

        SubmitVotesModal.IsLoading = false;
    }
    private async Task RefreshTheme()
    {
        Theme = await votingService.GetTheme();
        // If theme has not been set, default to use playlist description
        if (string.IsNullOrEmpty(Theme.Name) && Playlist != null)
            Theme.Name = Playlist.Description;
    }
    private async Task SubmitTheme()
    {
        var response = await votingService.SetTheme(FormData.AddedBy, FormData.Name);
        if (response.IsSuccess)
        {
            Theme.Name = FormData.Name;
            Theme.AddedBy = FormData.AddedBy;
        }
    }
}