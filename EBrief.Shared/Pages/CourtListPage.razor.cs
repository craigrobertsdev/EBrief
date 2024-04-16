using EBrief.Shared.Data;
using EBrief.Shared.Helpers;
using EBrief.Shared.Models;
using EBrief.Shared.Models.UI;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using System.Runtime.CompilerServices;

namespace EBrief.Shared.Pages;
public partial class CourtListPage
{
    public bool NewList { get; set; }
    private CourtList CourtList { get; set; } = default!;
    public CourtCode CourtCode { get; set; } = default!;
    public DateTime CourtDate { get; set; } = default!;
    public int CourtRoom { get; set; } = default!;
    private ElementReference? UnsavedChangesDialog { get; set; }
    private ElementReference? AddCaseFilesDialog { get; set; }
    private string CaseFilesToAdd { get; set; } = string.Empty;
    public Defendant? ActiveDefendant { get; set; }
    public event Func<Task>? OnDefendantChange;
    private string? _error;
    private readonly CourtListDataAccess _dataAccess = new();
    private bool _loading;

    protected override async Task OnInitializedAsync()
    {
        _loading = true;
        var queries = QueryHelpers.ParseQuery(NavManager.ToAbsoluteUri(NavManager.Uri).Query);
        var isNewList = queries.TryGetValue("newList", out _);
        CourtCode = Enum.Parse<CourtCode>(queries["courtCode"]!);
        CourtDate = DateTime.Parse(queries["courtDate"]!);
        CourtRoom = int.Parse(queries["courtRoom"]!);

        try
        {
            await LoadCourtList(isNewList, CourtCode, CourtDate, CourtRoom);
        }
        catch (Exception e)
        {
            _error = e.Message;
            return;
        }

        CourtList.GenerateInformations();
        CourtList.Defendants.Sort((a, b) => string.Compare(a.LastName, b.LastName, StringComparison.Ordinal));
        ActivateDefendant(CourtList.Defendants.First());
        _loading = false;
    }

    private async Task LoadCourtList(bool downloadDocuments, CourtCode courtCode, DateTime courtDate, int courtRoom)
    {
        var courtList = _dataAccess.GetCourtList(courtCode, courtDate, courtRoom)?.ToUIModel();

        if (courtList is null)
        {
            throw new Exception("Failed to load court list.");
        }

        CourtList = courtList;
        CourtList.CourtCode = courtCode;
        CourtList.CourtDate = courtDate;
        CourtList.CourtRoom = courtRoom;

        if (downloadDocuments)
        {
            await DownloadDocuments(courtList);
        }
    }

    private async Task AddCaseFiles()
    {
        CaseFilesToAdd = await JSRuntime.InvokeAsync<string>("openDialog", AddCaseFilesDialog);
        var caseFileNumbers = CaseFilesToAdd.Split(' ', '\n');
    }

    private async Task HandleReturnHome()
    {
        if (UnsavedChanges())
        {
            await JSRuntime.InvokeVoidAsync("openDialog", UnsavedChangesDialog);
        }

        NavManager.NavigateTo("/");
    }

    private async Task CloseUnsavedChangesDialog()
    {
        await JSRuntime.InvokeVoidAsync("closeDialog", UnsavedChangesDialog);
    }
    private void ActivateDefendant(Defendant defendant)
    {
        ActiveDefendant = defendant;
        if (ActiveDefendant.ActiveCaseFile is null)
        {
            ActiveDefendant.ActiveCaseFile = ActiveDefendant.CaseFiles.First();
        }

        OnDefendantChange?.Invoke();
    }

    private void CaseFileChanged()
    {
        ActivateDefendant(ActiveDefendant!);
    }

    private string IsSelected(Defendant defendant)
    {
        if (ActiveDefendant?.Id == defendant.Id)
        {
            return "!bg-sky-700";
        }

        return "hover:bg-gray-500";
    }

    private void SaveCourtList()
    {
        _dataAccess.UpdateCourtList(CourtList);
    }

    private void ExportCourtList()
    {
        var courtList = CourtList.SerialiseToJson();
        var fileName = $"Court {CourtRoom} {CourtCode} - {CourtList.CourtDate.Day} {CourtList.CourtDate:MMM} {CourtList.CourtDate.Year}.court";
        File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName), courtList);
    }

    private bool UnsavedChanges()
    {
        // Handles the case where something has gone wrong and the user wants to go back to the start
        if (_error is not null)
        {
            return false;
        }
        var courtList = _dataAccess.GetCourtList(CourtCode, CourtDate, CourtRoom)!;

        foreach (var caseFile in CourtList.GetCaseFiles())
        {
            var caseFileModel = courtList.CaseFiles.First(cf => cf.CaseFileNumber == caseFile.CaseFileNumber);
            if (caseFileModel.Notes != caseFile.Notes)
            {
                return true;
            }
        }

        return false;
    }

    private static async Task DownloadDocuments(CourtList courtList)
    {
        var client = new HttpClient();

        List<string> caseFileDocumentNames = [];
        List<string> occurrenceDocumentNames = [];

        foreach (var defendant in courtList.Defendants)
        {
            foreach (var caseFile in defendant.CaseFiles)
            {
                foreach (var caseFileDocument in caseFile.CaseFileDocuments)
                {
                    caseFileDocumentNames.Add(caseFileDocument.FileName);
                }

                foreach (var occurrenceDocument in caseFile.OccurrenceDocuments)
                {
                    occurrenceDocumentNames.Add(occurrenceDocument.FileName);
                }
            }
        }

        foreach (var document in caseFileDocumentNames)
        {
            await DownloadCaseFileDocument(document, client);
        }

        foreach (var document in occurrenceDocumentNames)
        {
            await DownloadEvidence(document, client);
        }
    }

    private static async Task DownloadCaseFileDocument(string fileName, HttpClient client)
    {
        var response = await client.GetAsync($"{AppConstants.ApiBaseUrl}/correspondence/?fileName={fileName}");
        var correspondenceDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EBrief", "correspondence");

        if (!Directory.Exists(correspondenceDirectory))
        {
            Directory.CreateDirectory(correspondenceDirectory);
        }

        if (response.IsSuccessStatusCode)
        {
            var pdfStream = await response.Content.ReadAsStreamAsync();
            var memoryStream = new MemoryStream();
            await pdfStream.CopyToAsync(memoryStream);

            using var fileStream = new FileStream($"{correspondenceDirectory}/{fileName}", FileMode.Create, FileAccess.Write);
            fileStream.Write(memoryStream.ToArray());
        }
    }

    private static async Task DownloadEvidence(string fileName, HttpClient client)
    {
        var response = await client.GetAsync($"{AppConstants.ApiBaseUrl}/evidence/?fileName={fileName}");
        var evidenceDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EBrief", "evidence");

        if (!Directory.Exists(evidenceDirectory))
        {
            Directory.CreateDirectory(evidenceDirectory);
        }

        if (response.IsSuccessStatusCode)
        {
            var pdfStream = await response.Content.ReadAsStreamAsync();
            var memoryStream = new MemoryStream();
            await pdfStream.CopyToAsync(memoryStream);

            using var fileStream = new FileStream($"{evidenceDirectory}/{fileName}", FileMode.Create, FileAccess.Write);
            fileStream.Write(memoryStream.ToArray());
        }
    }
}
