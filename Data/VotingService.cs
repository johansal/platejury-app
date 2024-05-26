using System.Data;
using Google.Cloud.Firestore;

namespace platejury_app.Data;

public class VotingService(ILogger<VotingService> logger)
{
    private readonly FirestoreDb db = FirestoreDb.Create("platejury-app");
    private readonly ILogger<VotingService> logger = logger;

    public async Task<VotingServiceResponse> AddVotes(string voterId, List<Item> votedItems) 
    {
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

        CollectionReference collection = db.Collection("votes");
        
        // Delete any existing votes for the same user
        QuerySnapshot docs = await collection.WhereEqualTo("resultDay", GetCurrentVotingDay()).WhereEqualTo("voterId",voterId).GetSnapshotAsync();
        foreach(var doc in docs)
        {
            logger.LogInformation("{datetime} Deleting existing votes {docId} for {voterId}", DateTime.Now, doc.Id, voterId);
            await collection.Document(doc.Id).DeleteAsync();
        }

        // Add new vote to collection.
        await collection.AddAsync(
            new {
                voterId,
                votedTracks = tracks,
                resultDay = GetCurrentVotingDay()
            }
        );
        logger.LogInformation(@"{datetime} Added votes for {voterId}", DateTime.Now, voterId);
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
        CollectionReference collection = db.Collection("votes");
        QuerySnapshot docs = await collection.WhereEqualTo("resultDay", votingDay).GetSnapshotAsync();

        var votes = new List<Votes>();
        foreach(var doc in docs)
        {
            votes.Add(
                new Votes
                {
                    Id = doc.Id,
                    VoterId = doc.GetValue<string>("voterId"),
                    VotedTracks = doc.GetValue<List<Dictionary<string, string>>>("votedTracks"),
                    ResultDay = doc.GetValue<DateTime>("resultDay")
                });
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

    public static DateTime GetCurrentVotingDay()
    {
        var today = DateTime.Today;
        //test result table any day by overwriting today with the voting day:
        //var today = new DateTime(2024, 2, 8);
        var daysUntilVotingDay = ((int) DayOfWeek.Thursday - (int) today.DayOfWeek + 7) % 7;
        DateTime votingDay = today.AddDays(daysUntilVotingDay);
        return DateTime.SpecifyKind(votingDay, DateTimeKind.Utc);
    }
}
