using EBrief.Data;
using EBrief.Helpers;
using EBrief.Models;
using EBrief.Models.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text;

namespace EBrief.Components.Pages;

public partial class Home {
    public ElementReference? NewCourtListDialog { get; set; }
    public ElementReference? PreviousCourtListDialog { get; set; }
    public string CaseFileNumbers { get; set; } = string.Empty;
    public DateTime? CourtDate { get; set; }
    public CourtCode? CourtCode { get; set; }
    public int? CourtRoom { get; set; }
    public string? _error;
    private List<CourtListEntry>? PreviousCourtLists { get; set; }
    private CourtListEntry? SelectedCourtList { get; set; }
    private async Task OpenNewCourtListDialog() {
        if (NewCourtListDialog is not null) {
            _error = null;
            await JSRuntime.InvokeVoidAsync("openDialog", NewCourtListDialog);
        }
    }
    private async Task OpenPreviousCourtListDialog() {
        var context = new ApplicationDbContext();
        PreviousCourtLists = context.CourtLists.Select(e => new CourtListEntry(e.CourtCode, e.CourtDate, e.CourtRoom)).ToList();
        if (PreviousCourtListDialog is not null) {
            _error = null;
            await JSRuntime.InvokeVoidAsync("openDialog", PreviousCourtListDialog);
        }
    }

    private void LoadPreviousCourtList() {
        if (SelectedCourtList is null) {
            _error = "Please select a court list.";
            return;
        }

        NavManager.NavigateTo($"/court-list?courtCode={SelectedCourtList.CourtCode}&courtDate={SelectedCourtList.CourtDate}&courtRoom={SelectedCourtList.CourtRoom}");
    }

    protected override void OnInitialized() {
        GetCmdArgs();
    }

    private async Task FetchCourtList() {
        _error = null;
        if (CourtDate is null) {
            _error = "Please select a court date.";
            return;
        }

        if (CourtCode is null) {
            _error = "Please select a court code.";
            return;
        }

        var client = new HttpClient();
        var caseFileNumbers = CaseFileNumbers.Split("\n").Where(e => !string.IsNullOrWhiteSpace(e));
        var response = await client.PostAsJsonAsync($"{AppConstants.ApiBaseUrl}/generate-case-files", caseFileNumbers);

        if (!response.IsSuccessStatusCode) {
            await JSRuntime.InvokeVoidAsync("alert", "Failed to fetch court list.");
            return;
        }

        var caseFiles = await response.Content.ReadFromJsonAsync<List<CaseFileModel>>();
        if (caseFiles is null) {
            _error = "Failed to fetch court list.";
            return;
        }

        var courtList = new CourtListModel {
            CaseFiles = caseFiles,
            CourtCode = CourtCode.Value,
            CourtDate = CourtDate.Value,
            CourtRoom = CourtRoom!.Value
        };

        courtList.CombineDefendantCaseFiles();

        var courtDbAccess = new CourtListDataAccess();
        courtDbAccess.SaveCourtList(courtList);

        NavManager.NavigateTo($"/court-list/?newList=true&courtCode={CourtCode}&courtRoom={CourtRoom}&courtDate={CourtDate}");
    }

    private async Task CloseLoadNewCourtListDialog() {
        if (NewCourtListDialog is not null) {
            await JSRuntime.InvokeVoidAsync("closeDialog", NewCourtListDialog);
        }
    }

    private async Task ClosePreviousCourtListDialog() {
        if (PreviousCourtListDialog is not null) {
            await JSRuntime.InvokeVoidAsync("closeDialog", PreviousCourtListDialog);
        }
    }

    private void GetCmdArgs() {
        var args = Environment.GetCommandLineArgs();
        if (args.Length > 1) {
            var stringBuilder = new StringBuilder();
            var streamReader = new StreamReader(args[1]);

            while (!streamReader.EndOfStream) {
                stringBuilder.AppendLine(streamReader.ReadLine());
            }

        }
    }

    private void HandleSelectCourtListEntry(CourtListEntry courtListEntry) {
        if (SelectedCourtList == courtListEntry) {
            SelectedCourtList = null;
            return;
        }

        SelectedCourtList = courtListEntry;
    }

    // This was implemented for the case of needing to specify which case file numbers related to the current court list
    // so they could be fetched from the database. Current implementation is that there will be the ability to only have one list at a time.
    //private string BuildCaseFileQueryString(IEnumerable<string> caseFileNumbers) {
    //    return string.Join("&", caseFileNumbers.Select(e => $"caseFileNumbers={e}"));
    //}

    record CourtListEntry(CourtCode CourtCode, DateTime CourtDate, int CourtRoom);

    private List<int> CourtRooms = [2, 3, 12, 15, 17, 18, 19, 20, 22, 23, 24];
}
