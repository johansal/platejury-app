using Microsoft.JSInterop;

using platejury_app.Data;

namespace platejury_app.Pages;

public partial class History
{
    private IEnumerable<HistoryTrack> histories = [];
    protected override async Task OnInitializedAsync()
    {
        histories = await historyService.GetAllHistory();
    }
}