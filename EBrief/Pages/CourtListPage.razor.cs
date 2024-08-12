using EBrief.Shared.Data;
using EBrief.Shared.Models.Data;
using EBrief.Shared.Services;
using EBrief.Shared.Models.UI;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using Radzen;
using EBrief.Shared.Models.Shared;

namespace EBrief.Pages;
public partial class CourtListPage : ICourtListPage
{
    [Inject] public IDataAccess DataAccess { get; set; } = default!;
    [Inject] public IFileService FileService { get; set; } = default!;
    [Inject] public AppState AppState { get; set; } = default!;
    public HttpService HttpService { get; set; } = default!;
    private CourtList CourtList { get; set; } = default!;
    public List<CourtSession> CourtSessions { get; set; } = [];
    private CourtCode CourtCode { get; set; } = default!;
    private DateTime CourtDate { get; set; } = default!;
    private int CourtRoom { get; set; } = default!;
    private bool IncludeDocuments { get; set; }
    private ElementReference? _unsavedChangesDialog { get; set; }
    private ElementReference? _addCaseFilesDialog { get; set; }
    private string CaseFilesToAdd { get; set; } = string.Empty;
    public Defendant? ActiveDefendant { get; set; }
    public event Func<Task>? OnDefendantChange;
    private string? _error;
    private string? _addCaseFilesError;
    private bool _loading;
    private bool _loadingNewCaseFiles;
    private readonly string CaseFileSelected = "bg-blue text-text hover:bg-blue";
    private readonly string CaseFileNotSelected = "hover:bg-slate-300";

    protected override async Task OnInitializedAsync()
    {
        await JSRuntime.InvokeVoidAsync("addSearchEventHandler");
        HttpService = new();
        _loading = true;
        var queries = QueryHelpers.ParseQuery(NavManager.ToAbsoluteUri(NavManager.Uri).Query);
        IncludeDocuments = queries.ContainsKey("includeDocuments");
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
        CourtSessions = GenerateCourtSessions();
        ActivateDefendant(CourtSessions[0].Defendants.First());

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

        CourtList = courtList;
        CourtList.CourtCode = courtCode;
        CourtList.CourtDate = courtDate;
        CourtList.CourtRoom = courtRoom;
    }

    private List<CourtSession> GenerateCourtSessions()
    {
        // iterate over the list of defendants and group them by the appearance time of their first case file in the list
        var courtSessions = CourtList.Defendants.SelectMany(d => d.CaseFiles)
            .GroupBy(cf => cf.Schedule.Last().HearingDate)
            .OrderBy(g => g.Key)
            .Select((g, i) => new CourtSession(i, g.Key))
            .ToList();

        foreach (var defendant in CourtList.Defendants)
        {
            var hearingTime = defendant.CaseFiles.First().Schedule.Last().HearingDate;
            courtSessions.First(cs => cs.SittingTime.TimeOfDay == hearingTime.TimeOfDay).Defendants.Add(defendant);
        }

        courtSessions.ForEach(cs => cs.Defendants.Sort((d1, d2) => d1.ListStart.CompareTo(d2.ListStart)));

        return courtSessions;
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
            UpdateCourtSessions(newCaseFiles.Select(cf => cf.Defendant).ToList());

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

    private void UpdateCourtSessions(List<Defendant> defendants)
    {
        foreach (var defendant in defendants)
        {
            var courtSession = CourtSessions.FirstOrDefault(cs => cs.SittingTime == defendant.CaseFiles.First().Schedule.Last().HearingDate);
            if (courtSession is null)
            {
                courtSession = new(CourtSessions.Count, defendant.CaseFiles.First().Schedule.Last().HearingDate);
            }

            courtSession.Defendants.Add(defendant);
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
            ReturnHome();
        }
    }

    private async Task SaveChanges()
    {
        await DataAccess.UpdateCourtList(CourtList);
        NavManager.NavigateTo("/");
    }

    private async void ReturnHome()
    {
        await JSRuntime.InvokeVoidAsync("removeSearchEventHandler");
        NavManager.NavigateTo("/");
    }

    private async Task CloseUnsavedChangesDialog()
    {
        await JSRuntime.InvokeVoidAsync("closeDialog", _unsavedChangesDialog);
    }

    public void ActivateDefendant(Defendant defendant)
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

    public bool IsSelected(Defendant defendant) => ActiveDefendant?.Id == defendant.Id;

    private bool IsSelected(CaseFile caseFile) => ActiveDefendant?.ActiveCaseFile?.CaseFileNumber == caseFile.CaseFileNumber;

    private void SaveCourtList()
    {
        DataAccess.UpdateCourtList(CourtList);

        foreach (var caseFile in CourtList.GetCaseFiles())
        {
            caseFile.Notes.HasChanged = false;
        }
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
}
