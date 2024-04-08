using CourtSystem.Data;
using CourtSystem.Helpers;
using CourtSystem.Models.UI;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CourtSystem.Components.Pages;
public partial class CourtListPage {
    [SupplyParameterFromQuery(Name = "newList")]
    public bool NewList { get; set; }
    private ElementReference? UnsavedChangesDialog { get; set; }
    private CourtList CourtList { get; set; } = default!;
    public Defendant? ActiveDefendant { get; set; }
    public event Func<Task>? OnDefendantChange;
    private string? _error;
    private readonly CourtListDataAccess _dataAccess = new();
    private bool _returnHomeButtonVisible;
    private bool _loading;

    protected override async Task OnInitializedAsync() {
        _loading = true;
        if (NewList) {
            try {
                await LoadNewCourtList();
            }
            catch (Exception e) {
                _error = e.Message;
                return;
            }
        }
        else {
            try {
                LoadPreviousCourtList();
            }
            catch (Exception e) {
                _error = e.Message;
                return;
            }
        }

        CourtList.GenerateInformations();
        CourtList.Defendants.Sort((a, b) => string.Compare(a.LastName, b.LastName, StringComparison.Ordinal));
        ActivateDefendant(CourtList.Defendants.First());
        _loading = false;
    }

    private async Task LoadNewCourtList() {
        var courtList = _dataAccess.GetCourtList()?.ToUIModel();

        if (courtList is null) {
            throw new Exception("Failed to load court list.");
        }

        CourtList = courtList;

        await DownloadDocuments(courtList);
    }

    private void LoadPreviousCourtList() {
        var courtList = _dataAccess.GetCourtList()?.ToUIModel();

        if (courtList is null) {
            _returnHomeButtonVisible = true;
            throw new Exception("No previous court list in database");
        }

        CourtList = courtList;
    }

    private async Task HandleReturnHome() {
        if (UnsavedChanges()) {
            await JSRuntime.InvokeVoidAsync("openDialog", UnsavedChangesDialog);
        }

        NavManager.NavigateTo("/");
    }

    private async Task CloseUnsavedChangesDialog() {
        await JSRuntime.InvokeVoidAsync("closeDialog", UnsavedChangesDialog);
    }   
    private void ActivateDefendant(Defendant defendant) {
        ActiveDefendant = defendant;
        if (ActiveDefendant.ActiveCaseFile is null) {
            ActiveDefendant.ActiveCaseFile = ActiveDefendant.CaseFiles.First();
        }

        OnDefendantChange?.Invoke();
    }

    private void CaseFileChanged() {
        ActivateDefendant(ActiveDefendant!);
    }

    private string IsSelected(Defendant defendant) {
        if (ActiveDefendant?.Id == defendant.Id) {
            return "!bg-sky-700";
        }

        return "hover:bg-gray-500";
    }

    private void SaveCourtList() {
        _dataAccess.UpdateCourtList(CourtList);
    }

    private bool UnsavedChanges() {
        if (_returnHomeButtonVisible) {
            return false;
        }

        var courtList = _dataAccess.GetCourtList()!;

        foreach (var caseFile in CourtList.GetCaseFiles()) {
            var caseFileModel = courtList.CaseFiles.First(cf => cf.CaseFileNumber == caseFile.CaseFileNumber);
            if (caseFileModel.Notes != caseFile.Notes) {
                return true;
            }
        }

        return false;
    }

    private static async Task DownloadDocuments(CourtList courtList) {
        var client = new HttpClient();

        List<string> caseFileDocumentNames = [];
        List<string> occurrenceDocumentNames = [];

        foreach (var defendant in courtList.Defendants) {
            foreach (var caseFile in defendant.CaseFiles) {
                foreach (var caseFileDocument in caseFile.CaseFileDocuments) {
                    caseFileDocumentNames.Add(caseFileDocument.FileName);
                }

                foreach (var occurrenceDocument in caseFile.OccurrenceDocuments) {
                    occurrenceDocumentNames.Add(occurrenceDocument.FileName);
                }
            }
        }

        foreach (var document in caseFileDocumentNames) {
            await DownloadCaseFileDocument(document, client);
        }

        foreach (var document in occurrenceDocumentNames) {
            await DownloadEvidence(document, client);
        }
    }

    private static async Task DownloadCaseFileDocument(string fileName, HttpClient client) {
        var response = await client.GetAsync($"{AppConstants.ApiBaseUrl}/correspondence/?fileName={fileName}");
        var correspondenceDirectory = Path.Combine(FileSystem.AppDataDirectory, "correspondence");

        if (!Directory.Exists(correspondenceDirectory)) {
            Directory.CreateDirectory(correspondenceDirectory);
        }

        if (response.IsSuccessStatusCode) {
            var pdfStream = await response.Content.ReadAsStreamAsync();
            var memoryStream = new MemoryStream();
            await pdfStream.CopyToAsync(memoryStream);

            using var fileStream = new FileStream($"{correspondenceDirectory}/{fileName}", FileMode.Create, FileAccess.Write);
            fileStream.Write(memoryStream.ToArray());
        }
    }

    private static async Task DownloadEvidence(string fileName, HttpClient client) {
        var response = await client.GetAsync($"{AppConstants.ApiBaseUrl}/evidence/?fileName={fileName}");
        var evidenceDirectory = Path.Combine(FileSystem.AppDataDirectory, "evidence");

        if (!Directory.Exists(evidenceDirectory)) {
            Directory.CreateDirectory(evidenceDirectory);
        }

        if (response.IsSuccessStatusCode) {
            var pdfStream = await response.Content.ReadAsStreamAsync();
            var memoryStream = new MemoryStream();
            await pdfStream.CopyToAsync(memoryStream);

            using var fileStream = new FileStream($"{evidenceDirectory}/{fileName}", FileMode.Create, FileAccess.Write);
            fileStream.Write(memoryStream.ToArray());
        }
    }
}
