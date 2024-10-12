using Google.Cloud.Firestore;
using System.Text.Json;

namespace platejury_app.Data;

public class HistoryService(ILogger<HistoryService> logger, PlaylistService playListService, string projectName, string collectionName)
{
    private readonly FirestoreDb db = FirestoreDb.Create(projectName);
    private readonly ILogger<HistoryService> logger = logger;
    
    public async Task<IEnumerable<HistoryTrack>> GetAllHistory()
    {
        CollectionReference collection = db.Collection(collectionName);
        QuerySnapshot docs = await collection.GetSnapshotAsync();
        logger.LogDebug("History snapshot contains {amount} entries.", docs.Count);
        IEnumerable<HistoryTrack> history = [];

        foreach(var doc in docs)
        {
            history = history.Append(
                new HistoryTrack
                {
                    Id = doc.Id,
                    UserId = doc.GetValue<string>("userId"),
                    UserName = doc.GetValue<string>("userName"),
                    TrackId = doc.GetValue<string>("trackId"),
                    TrackName = doc.GetValue<string>("trackName"),
                    Points = doc.GetValue<int>("points"),
                    Position = doc.GetValue<int>("position"),
                    ResultDay = doc.GetValue<DateTime>("resultDay")
                });
        }
        return history;
    }
    public async Task<Dictionary<string, string>> ParseDisplayNames(List<string> uids)
    {
        return await playListService.GetUserDisplayNames(uids);
    }
    public async Task AddTracksFromResults(IEnumerable<ResultRow> rows, DateTime rDay)
    {
        // Check if we already have these in results
        if(!rows.Any())
            return;

        CollectionReference collection = db.Collection(collectionName);
        Query query = collection.WhereEqualTo("resultDay", rDay).Limit(1);
        var result = await query.GetSnapshotAsync();
        if(result.Count == 1)
        {
            return;
        }
        
        logger.LogInformation("Results are not in history, adding...");
        
        // Get displayNames
        var trackNames = await playListService.GetTackDisplayNames(rows.Select(x => x.TrackId).ToList());
        var userNames = await playListService.GetUserDisplayNames(rows.Select(x => x.AddedBy).ToList());

        // Add results
        var pos = 1;
        foreach(var row in rows.OrderByDescending(_ => _))
        {
            await collection.AddAsync(
                new {
                    userId = row.AddedBy,
                    userName = userNames[row.AddedBy],
                    trackId = row.TrackId,
                    trackName = trackNames[row.TrackId],
                    points = row.GetTotal(),
                    position = pos,
                    resultDay = rDay
                }
            );
            pos++;
        }
        logger.LogInformation("Added results to history.");
    }
}