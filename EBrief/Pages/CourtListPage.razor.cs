﻿using EBrief.Data;
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
    [Inject] public TooltipService TooltipService { get; set; } = default!;
    [Inject] public IDataAccess DataAccess { get; set; } = default!;
    [Inject] public IFileService FileService { get; set; } = default!;
    public HttpService HttpService { get; set; } = default!;
    public bool NewList { get; set; }
    private CourtList CourtList { get; set; } = default!;
    private List<CourtSitting> CourtSittings { get; set; } = [];
    public CourtCode CourtCode { get; set; } = default!;
    public DateTime CourtDate { get; set; } = default!;
    public int CourtRoom { get; set; } = default!;
    private bool _includeDocuments;
    private ElementReference? UnsavedChangesDialog { get; set; }
    private ElementReference? AddCaseFilesDialog { get; set; }
    private Dictionary<string, ElementReference?> CaseFileHeaderRefs { get; set; } = [];
    private string RenderingCaseFileNumber { get; set; } = string.Empty;
    private ElementReference CaseFileHeaderRef
    {
        set { CaseFileHeaderRefs.TryAdd(RenderingCaseFileNumber, value); }
    }
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
        if (AddCaseFilesDialog is not null)
        {
            _error = null;
            await JSRuntime.InvokeAsync<string>("openDialog", AddCaseFilesDialog);
        }
    }

    private async Task CloseAddCaseFilesDialog()
    {
        CaseFilesToAdd = string.Empty;
        await JSRuntime.InvokeVoidAsync("closeDialog", AddCaseFilesDialog);
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
            var newCaseFiles = await HttpService.GetCaseFiles(newCaseFileNumbers);

            await DataAccess.AddCaseFiles(newCaseFiles, CourtList);
            CourtList.AddCaseFiles(newCaseFiles.ToUIModels());
            await JSRuntime.InvokeVoidAsync("closeDialog", AddCaseFilesDialog);
            _loadingNewCaseFiles = false;
        }
        catch (Exception e)
        {
            _addCaseFilesError = e.Message;
            return;
        }

    }

    private async Task HandleReturnHome()
    {
        if (await UnsavedChanges())
        {
            await JSRuntime.InvokeVoidAsync("openDialog", UnsavedChangesDialog);
        }

        NavManager.NavigateTo("/");
    }

    private async Task CloseUnsavedChangesDialog()
    {
        await JSRuntime.InvokeVoidAsync("closeDialog", UnsavedChangesDialog);
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

    private async Task<bool> UnsavedChanges()
    {
        // Handles the case where something has gone wrong and the user wants to go back to the start
        if (_error is not null)
        {
            return false;
        }
        var courtList = await DataAccess.GetCourtList(CourtCode, CourtDate, CourtRoom)!;

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
