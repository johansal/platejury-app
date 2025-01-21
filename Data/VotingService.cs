using Google.Cloud.Firestore;

namespace platejury_app.Data;

public class VotingService(ILogger<VotingService> logger, string projectName, string collectionName)
{
    private readonly FirestoreDb db = FirestoreDb.Create(projectName);
    private readonly ILogger<VotingService> logger = logger;

    public async Task<VotingServiceResponse> AddVotes(string voterId, List<Item> votedItems) 
    {
        var votingDay = GetCurrentVotingDay();
        var tracks = new List<Dictionary<string, string>>();
        foreach(var item in votedItems)
        {
            if(voterId == item.AddedBy.Id)
            {
                logger.LogWarning("Votes rejected for {voterid}, cannot vote for own song!", voterId);
                return new() {
                    IsSuccess = false,
                    Message = "Cannot vote own song!"
                };
            }

            var track = new Dictionary<string, string>
            {
                ["trackId"] = item.Track.Id,
                ["addedBy"] = item.AddedBy.Id
            };
            tracks.Add(track);
        }

        CollectionReference collection = db.Collection(collectionName);
        
        // Delete any existing votes for the same user
        QuerySnapshot docs = await collection.WhereEqualTo("resultDay", votingDay).WhereEqualTo("voterId",voterId).GetSnapshotAsync();
        foreach(var doc in docs)
        {
            logger.LogInformation("{datetime} Deleting existing votes {docId} for {voterId}", DateTime.Now, doc.Id, voterId);
            await collection.Document(doc.Id).DeleteAsync();
        }

        // Add new vote to collection.
        var vote = await collection.AddAsync(
            new {
                voterId,
                votedTracks = tracks,
                resultDay = votingDay
            }
        );
        logger.LogInformation(@"{datetime} Added votes {voteId} for {voterId}", DateTime.Now, vote.Id, voterId);
        return new() {
            IsSuccess = true
        };
    }
    public async Task<List<Votes>> GetVotes()
    {
        return await GetVotes(GetCurrentVotingDay());
    }
    public async Task<List<Votes>> GetVotes(DateTime votingDay)
    {
        CollectionReference collection = db.Collection(collectionName);
        QuerySnapshot docs = await collection.WhereEqualTo("resultDay", votingDay).GetSnapshotAsync();

        var votes = new List<Votes>();
        List<string> voterIds = [];
        foreach(var doc in docs)
        {
            // check fort duplicate votes
            var voterId = doc.GetValue<string>("voterId");
            if (voterIds.Contains(voterId))
            {
                logger.LogError("{datetime} Duplicate votes found for {voterid}, skipping document {id}!", DateTime.Now, doc.GetValue<string>("voterId"), doc.Id);
            }
            else
            {
                votes.Add(
                    new Votes
                    {
                        Id = doc.Id,
                        VoterId = voterId,
                        VotedTracks = doc.GetValue<List<Dictionary<string, string>>>("votedTracks"),
                        ResultDay = doc.GetValue<DateTime>("resultDay")
                    });
                voterIds.Add(voterId);
            }
        }
        return votes;
    }
    public IEnumerable<ResultRow> GetResults(List<Votes> votes)
    {
        IEnumerable<ResultRow> results = [];
        foreach(var vote in votes)
        {
            for(int i = 0; i < vote.VotedTracks.Count; i++)
            {
                ResultRow? entry = results.FirstOrDefault(x => x.AddedBy.Equals(vote.VotedTracks[i]["addedBy"]));
                if(entry == null)
                {
                    entry = new(){
                        TrackId = vote.VotedTracks[i]["trackId"],
                        AddedBy = vote.VotedTracks[i]["addedBy"]
                    };
                    results = results.Append(entry);
                }
                var points = 5-i;
                entry.AddToTotal(points);
                entry.Points[vote.VoterId] = points;
            }
        }
        return results;
    }
    /// <summary>
    /// Get the date for the ongoing voting periods voting day (next thursday).
    /// </summary>
    /// <returns>Today if it is currently thursday; otherwise returns next Thursday.</returns>
    public static DateTime GetCurrentVotingDay()
    {
        var today = DateTime.Today;
        //test result table any day by overwriting today with the voting day:
        //var today = new DateTime(2024, 2, 8);
        var daysUntilVotingDay = ((int) DayOfWeek.Thursday - (int) today.DayOfWeek + 7) % 7;
        DateTime votingDay = today.AddDays(daysUntilVotingDay);
        return DateTime.SpecifyKind(votingDay, DateTimeKind.Utc);
    }
    /// <summary>
    /// Queries database for the date of first existing results that is older than the startAfter parameter.
    /// </summary>
    /// <param name="startAfter"></param>
    /// <returns>resultDay value of the query result; if query returns empty collection this defaults to last thursday.</returns>
    public async Task<DateTime> GetLatestResultDayAsync(DateTime startAfter)
    {
        CollectionReference collection = db.Collection(collectionName);
        Query query = collection.OrderByDescending("resultDay").StartAfter(startAfter).Limit(1);
        var docs = await query.GetSnapshotAsync();

        if(docs.Count == 1)
        {
            return docs[0].GetValue<DateTime>("resultDay");
        }
        else {
            logger.LogWarning("Could not find earlier results from db, defaulting to last week!");
            logger.LogDebug("{count}", docs.Count);
        }
        // Default to last weeks thursday
        var daysUntilThursday = ((int) DayOfWeek.Thursday - (int) startAfter.DayOfWeek + 7) % 7;
        return startAfter.AddDays(daysUntilThursday-7);
    }
}
