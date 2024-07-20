using EBrief.Data;
using EBrief.Helpers;
using EBrief.Models;
using EBrief.Models.Data;
using EBrief.Models.UI;
using EBrief.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using Radzen;
using System.Net.Http;

namespace EBrief.Pages;
public partial class CourtListPage
{
    [Inject] public IDataAccess DataAccess { get; set; } = default!;
    [Inject] public IFileService FileService { get; set; } = default!;
    [Inject] public AppState AppState { get; set; } = default!;
    public HttpService HttpService { get; set; } = default!;
    private CourtList CourtList { get; set; } = default!;
    private List<CourtSitting> CourtSittings { get; set; } = [];
    private CourtCode CourtCode { get; set; } = default!;
    private DateTime CourtDate { get; set; } = default!;
    private int CourtRoom { get; set; } = default!;
    private bool _includeDocuments;
    private ElementReference? _unsavedChangesDialog { get; set; }
    private ElementReference? _addCaseFilesDialog { get; set; }
    private string CaseFilesToAdd { get; set; } = string.Empty;
    public Defendant? ActiveDefendant { get; set; }
    public event Func<Task>? OnDefendantChange;
    private string? _error;
    private string? _addCaseFilesError;
    private bool _loading;
    private bool _loadingNewCaseFiles;

    protected override async Task OnInitializedAsync()
    {
        HttpService = new();
        _loading = true;
        var queries = QueryHelpers.ParseQuery(NavManager.ToAbsoluteUri(NavManager.Uri).Query);
        _includeDocuments = queries.ContainsKey("includeDocuments");
        CourtCode = Enum.Parse<CourtCode>(queries["courtCode"]!);
        CourtDate = DateTime.Parse(queries["courtDate"]!);
        CourtRoom = int.Parse(queries["courtRoom"]!);

        try
        {
            await LoadCourtList(CourtCode, CourtDate, CourtRoom);
        }
        catch (Exception e)
        {
            _error = e.Message;
            return;
        }

        CourtList.GenerateInformations();
        CourtList.Defendants.Sort((a, b) => string.Compare(a.LastName, b.LastName, StringComparison.Ordinal));
        CourtSittings = GenerateCourtSittings();
        ActivateDefendant(CourtList.Defendants.First());

        AppState.CurrentCourtList = CourtList;
        _loading = false;
    }

    private async Task LoadCourtList(CourtCode courtCode, DateTime courtDate, int courtRoom)
    {
        var courtList = (await DataAccess.GetCourtList(courtCode, courtDate, courtRoom))?.ToUIModel();

        if (courtList is null)
        {
            throw new Exception("Failed to load court list.");
        }

        if (_includeDocuments)
        {
            await DownloadDocuments();
        }

        CourtList = courtList;
        CourtList.CourtCode = courtCode;
        CourtList.CourtDate = courtDate;
        CourtList.CourtRoom = courtRoom;
    }

    private List<CourtSitting> GenerateCourtSittings()
    {
        // iterate over the list of defendants and group them by the appearance time of their first case file in the list
        var courtSittings = CourtList.Defendants.SelectMany(d => d.CaseFiles)
            .GroupBy(cf => cf.Schedule.Last().HearingDate)
            .OrderBy(g => g.Key)
            .Select((g, i) => new CourtSitting(i, g.Key))
            .ToList();

        foreach (var defendant in CourtList.Defendants)
        {
            var hearingTime = defendant.CaseFiles.First().Schedule.Last().HearingDate;
            courtSittings.First(cs => cs.SittingTime.TimeOfDay == hearingTime.TimeOfDay).Defendants.Add(defendant);
        }

        return courtSittings;
    }

    private async Task OpenAddCaseFilesDialog()
    {
        if (_addCaseFilesDialog is not null)
        {
            _error = null;
            await JSRuntime.InvokeAsync<string>("openDialog", _addCaseFilesDialog);
        }
    }

    private async Task CloseAddCaseFilesDialog()
    {
        CaseFilesToAdd = string.Empty;
        await JSRuntime.InvokeVoidAsync("closeDialog", _addCaseFilesDialog);
    }

    private async Task AddCaseFiles()
    {
        _loadingNewCaseFiles = true;
        var caseFileNumbers = CaseFilesToAdd.Split(' ', '\n').Where(e => !string.IsNullOrWhiteSpace(e));
        var newCaseFileNumbers = caseFileNumbers.Except(CourtList.GetCaseFiles().Select(cf => cf.CaseFileNumber)).ToList();

        if (newCaseFileNumbers.Count == 0)
        {
            _addCaseFilesError = "All of those case files are in the list already";
            return;
        }

        try
        {
            _addCaseFilesError = null;
            var newCaseFileModels = await HttpService.GetCaseFiles(newCaseFileNumbers, CourtDate);
            await DataAccess.AddCaseFiles(newCaseFileModels, CourtList);
            var newCaseFiles = newCaseFileModels.ToUIModels();
            newCaseFiles.AddReferenceToDefendants();
            CourtList.AddCaseFiles(newCaseFiles);
            UpdateCourtSittings(newCaseFiles.Select(cf => cf.Defendant).ToList());

            CaseFilesToAdd = string.Empty;
            await JSRuntime.InvokeVoidAsync("closeDialog", _addCaseFilesDialog);
            _loadingNewCaseFiles = false;
        }
        catch
        {
            _addCaseFilesError = "Failed to add case files";
            _loadingNewCaseFiles = false;
            return;
        }

    }

    private void UpdateCourtSittings(List<Defendant> defendants)
    {
        foreach (var defendant in defendants)
        {
            var courtSitting = CourtSittings.FirstOrDefault(cs => cs.SittingTime == defendant.CaseFiles.First().Schedule.Last().HearingDate);
            if (courtSitting is null)
            {
                courtSitting = new(CourtSittings.Count, defendant.CaseFiles.First().Schedule.Last().HearingDate);
            }

            courtSitting.Defendants.Add(defendant);
        }

        OnDefendantChange?.Invoke();
    }

    private async Task HandleReturnHome()
    {
        if (UnsavedChanges())
        {
            await JSRuntime.InvokeVoidAsync("openDialog", _unsavedChangesDialog);
        }
        else
        {
            NavManager.NavigateTo("/");
        }
    }

    private async Task SaveChanges()
    {
        await DataAccess.UpdateCourtList(CourtList);
        NavManager.NavigateTo("/");
    }

    private async Task CloseUnsavedChangesDialog()
    {
        await JSRuntime.InvokeVoidAsync("closeDialog", _unsavedChangesDialog);
    }

    internal void ActivateDefendant(Defendant defendant)
    {
        ActiveDefendant = defendant;
        if (ActiveDefendant.ActiveCaseFile is null)
        {
            ActiveDefendant.ActiveCaseFile = ActiveDefendant.CaseFiles.First();
        }

        OnDefendantChange?.Invoke();
        StateHasChanged();
    }

    private void CaseFileChanged(CaseFile caseFile)
    {
        if (ActiveDefendant is not null)
        {
            ActiveDefendant.ActiveCaseFile = caseFile;
        }
        ActivateDefendant(ActiveDefendant!);
    }

    internal string IsSelected(Defendant defendant)
    {
        if (ActiveDefendant?.Id == defendant.Id)
        {
            return "!bg-sky-700";
        }

        return "hover:bg-gray-500";
    }

    private string IsSelected(CaseFile caseFile)
    {
        if (ActiveDefendant?.ActiveCaseFile?.CaseFileNumber == caseFile.CaseFileNumber)
        {
            return "!bg-sky-700";
        }

        return "hover:bg-gray-500";
    }

    private void SaveCourtList()
    {
        DataAccess.UpdateCourtList(CourtList);
    }

    private async Task ExportCourtList()
    {
        await FileService.SaveFile(CourtList);
    }

    private bool UnsavedChanges()
    {
        // Handles the case where something has gone wrong and the user wants to go back to the start
        if (_error is not null)
        {
            return false;
        }

        foreach (var caseFile in CourtList.GetCaseFiles())
        {
            if (caseFile.Notes.HasChanged)
            {
                return true;
            }
        }

        return false;
    }

    private async Task DownloadDocuments()
    {
        var client = new HttpClient();

        List<string> caseFileDocumentNames = [];
        List<string> occurrenceDocumentNames = [];

        foreach (var defendant in CourtList.Defendants)
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

    private async Task DownloadCaseFileDocument(string fileName, HttpClient client)
    {
        var response = await client.GetAsync($"{AppConstants.ApiBaseUrl}/correspondence/?fileName={fileName}");
        FileService.CreateCorrespondenceDirectory();
        if (response.IsSuccessStatusCode)
        {
            var pdfStream = await response.Content.ReadAsStreamAsync();
            await FileService.SaveDocument(pdfStream, fileName, FolderType.Correspondence);
        }
    }

    private async Task DownloadEvidence(string fileName, HttpClient client)
    {
        var response = await client.GetAsync($"{AppConstants.ApiBaseUrl}/evidence/?fileName={fileName}");
        FileService.CreateEvidenceDirectory();
        if (response.IsSuccessStatusCode)
        {
            var pdfStream = await response.Content.ReadAsStreamAsync();
            await FileService.SaveDocument(pdfStream, fileName, FolderType.Evidence);
        }
    }
}
