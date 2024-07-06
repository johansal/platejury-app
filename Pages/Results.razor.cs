using Microsoft.JSInterop;
using platejury_app.Data;

namespace platejury_app.Pages;

public partial class Results
{
    private Dictionary<string, string> DisplayNames = [];
    private Dictionary<string, string> TrackNames = [];
    private List<Votes> Votes = [];
    private IEnumerable<ResultRow> ResultSet = [];
    //init result day to last week
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
        if (Votes.Count > 0) {
            ResultSet = votingService.GetResults(Votes).OrderByDescending(_ => _);
            DisplayNames = await playlistService.GetUserDisplayNames(ResultSet.Select(x => x.AddedBy).ToList());
            TrackNames = await playlistService.GetTackDisplayNames(ResultSet.Select(x => x.TrackId).ToList());
        }
    }
}