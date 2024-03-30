using CourtSystem.Data;
using CourtSystem.Helpers;
using CourtSystem.Models.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace CourtSystem.Components.Pages;

public partial class Home {
    public ElementReference? LoadNewCourtListDialog { get; set; }
    public ElementReference? LoadPreviousCourtListDialog { get; set; }
    public string CaseFileNumbers { get; set; } = string.Empty;
    private string? _error;
    private async Task OpenNewCourtListDialog() {
        if (LoadNewCourtListDialog is not null) {
            _error = null;
            await JSRuntime.InvokeVoidAsync("openDialog", LoadNewCourtListDialog);
        }
    }

    private void LoadPreviousCourtList() {
        NavManager.NavigateTo("/court-list?newList=false");
    }

    private async Task FetchCourtList() {
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
            CaseFiles = caseFiles
        };

        courtList.CombineDefendantCaseFiles();

        var courtDbAccess = new CourtListDataAccess();

        courtDbAccess.ClearDatabase();
        courtDbAccess.SaveCourtList(courtList);

        NavManager.NavigateTo($"/court-list/?newList=true");
    }

    private async Task CloseLoadNewCourtListDialog() {
        if (LoadNewCourtListDialog is not null) {
            await JSRuntime.InvokeVoidAsync("closeDialog", LoadNewCourtListDialog);
        }
    }

    // This was implemented for the case of needing to specify which case file numbers related to the current court list
    // so they could be fetched from the database. Current implementation is that there will be the ability to only have one list at a time.
    //private string BuildCaseFileQueryString(IEnumerable<string> caseFileNumbers) {
    //    return string.Join("&", caseFileNumbers.Select(e => $"caseFileNumbers={e}"));
    //}
}