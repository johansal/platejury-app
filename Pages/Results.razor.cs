using Microsoft.JSInterop;
using platejury_app.Data;

namespace platejury_app.Pages;

public partial class Results
{
    private Dictionary<string, string> DisplayNames = [];
    private Dictionary<string, string> TrackNames = [];
    private List<Votes> Votes = [];
    private IEnumerable<ResultRow> ResultSet = [];
    private Theme Theme = new();
    private DateTime resultDay;
    protected override async Task OnInitializedAsync()
    {
        resultDay = VotingService.GetCurrentVotingDay();
        await PreviousWeek();
    }
    private async Task PreviousWeek() 
    {
        resultDay = await votingService.GetLatestResultDayAsync(resultDay);
        Votes =  await votingService.GetVotes(resultDay);
        if (Votes.Count > 0)
        {
            ResultSet = votingService.GetResults(Votes).OrderByDescending(_ => _);
            DisplayNames = await playlistService.GetUserDisplayNames([.. ResultSet.Select(x => x.AddedBy)]);
            TrackNames = await playlistService.GetTackDisplayNames([.. ResultSet.Select(x => x.TrackId)]);
            //make sure ResultSet has been added to history
            await historyService.AddTracksFromResults(ResultSet, resultDay);
            //get the theme - will return default if not found
            Theme = await votingService.GetTheme(resultDay);
            if (string.IsNullOrEmpty(Theme.Name))
            {
                // add placeholder for the theme with correct timestamp, so its easier to edit in later
                await votingService.SetTheme(string.Empty, string.Empty, resultDay);
            }
        }
    }
}