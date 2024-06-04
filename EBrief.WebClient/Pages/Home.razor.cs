using EBrief.WebClient.Data;
using EBrief.WebClient.Models;
using EBrief.WebClient.Models.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace EBrief.WebClient.Pages;

public partial class Home
{
    [Inject]
    public LocalStorage _localStorage { get; set; } = default!;
    public ElementReference? NewCourtListDialog { get; set; }
    public ElementReference? PreviousCourtListDialog { get; set; }
    public string CaseFileNumbers { get; set; } = string.Empty;
    public DateTime? CourtDate { get; set; }
    public CourtCode? CourtCode { get; set; }
    public int? CourtRoom { get; set; }
    public string? _error;
    private List<CourtListEntry>? PreviousCourtLists { get; set; }
    private CourtListEntry? SelectedCourtList { get; set; }

    private async Task OpenNewCourtListDialog()
    {
        if (NewCourtListDialog is not null)
        {
            _error = null;
            await JSRuntime.InvokeVoidAsync("openDialog", NewCourtListDialog);
        }
    }
    private async Task OpenPreviousCourtListDialog()
    {
        PreviousCourtLists = (await _localStorage.GetPreviousCourtLists()).Select(e => new CourtListEntry(e.CourtCode, e.CourtDate, e.CourtRoom)).ToList();
        if (PreviousCourtListDialog is not null)
        {
            _error = null;
            await JSRuntime.InvokeVoidAsync("openDialog", PreviousCourtListDialog);
        }
    }

    private void LoadPreviousCourtList()
    {
        if (SelectedCourtList is null)
        {
            _error = "Please select a court list.";
            return;
        }

        NavManager.NavigateTo($"/court-list?courtCode={SelectedCourtList.CourtCode}&courtDate={SelectedCourtList.CourtDate}&courtRoom={SelectedCourtList.CourtRoom}");
    }

    private async Task DeletePreviousCourtList()
    {
        if (SelectedCourtList is null || PreviousCourtLists is null)
        {
            return;
        }

        try
        {
            await _localStorage.DeleteCourtList(SelectedCourtList);
        }
        catch (Exception e)
        {
            _error = e.InnerException?.Message ?? e.Message;
            return;
        }

        PreviousCourtLists.Remove(SelectedCourtList);
    }

    //protected override void OnInitialized() {
    //    var args = Environment.GetCommandLineArgs();

    //    if (args.Length == 0) {
    //        return;
    //    }

    //    string filePath = string.Empty;
    //    foreach (var arg in args[1..]) {
    //        filePath += arg + " ";
    //    }

    //    var json = File.ReadAllText(filePath);
    //    var courtList = JsonSerializer.Deserialize<CourtListModel>(json);
    //    var context = new ApplicationDbContext();
    //}

    private async Task FetchCourtList()
    {
        _error = null;
        if (CourtDate is null)
        {
            _error = "Please select a court date.";
            return;
        }

        if (CourtCode is null)
        {
            _error = "Please select a court code.";
            return;
        }

        var caseFileNumbers = CaseFileNumbers.Split(' ', '\n').Where(e => !string.IsNullOrWhiteSpace(e));
        try
        {
            var caseFiles = DummyData.GenerateCaseFiles(caseFileNumbers);
            if (caseFiles is null)
            {
                _error = "Failed to fetch court list.";
                return;
            }

            var courtList = new CourtListModel
            {
                CaseFiles = caseFiles,
                CourtCode = CourtCode.Value,
                CourtDate = CourtDate.Value,
                CourtRoom = CourtRoom!.Value
            };

            courtList.CombineDefendantCaseFiles();

            try
            {
                await _localStorage.SaveCourtList(courtList.ToUIModel());
            }
            catch (Exception e)
            {
                _error = e.InnerException?.Message ?? e.Message;
                return;
            }

            NavManager.NavigateTo($"/court-list/?newList=true&courtCode={CourtCode}&courtRoom={CourtRoom}&courtDate={CourtDate}");
        }
        catch (Exception e)
        {
            _error = e.InnerException?.Message ?? e.Message;
        }

    }

    private async Task CloseLoadNewCourtListDialog()
    {
        if (NewCourtListDialog is not null)
        {
            await JSRuntime.InvokeVoidAsync("closeDialog", NewCourtListDialog);
        }
    }

    private async Task ClosePreviousCourtListDialog()
    {
        if (PreviousCourtListDialog is not null)
        {
            await JSRuntime.InvokeVoidAsync("closeDialog", PreviousCourtListDialog);
        }
    }

    private void HandleSelectCourtListEntry(CourtListEntry courtListEntry)
    {
        if (SelectedCourtList == courtListEntry)
        {
            SelectedCourtList = null;
            return;
        }

        SelectedCourtList = courtListEntry;
    }

    private List<int> CourtRooms = [2, 3, 12, 15, 17, 18, 19, 20, 22, 23, 24];
}
