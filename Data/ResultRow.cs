namespace platejury_app.Data;

public class ResultRow : IComparable
{
    public required string TrackId {get; set;}
    public required string AddedBy {get; set;}
    public Dictionary<string, int> Points = [];
    private int totalPnts = 0;
    public void AddToTotal(int p)
    {
        totalPnts += p;
    }
    public int GetTotal()
    {
        return totalPnts;
    }

    public int CompareTo(object? obj)
    {
        if(obj != null)
        {
            var o = (ResultRow) obj;
            var ret = GetTotal() - o.GetTotal();
            // tie breaker for total points is more better points, eg. 2x5p. wins 1x5p.
            if(ret == 0)
            {
                for(int n = 5; n > 0; n--)
                {
                    ret = Points.Count(x => x.Value == n) - o.Points.Count(x => x.Value == n);
                    if(ret != 0)
                        return ret;
                }
                return 0;
            }
            else 
            {
                return ret;
            }
        }
        return -1;
    }
}
