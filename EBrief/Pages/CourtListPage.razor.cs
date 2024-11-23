using EBrief.Shared.Data;
using EBrief.Shared.Models.Data;
using EBrief.Shared.Services;
using EBrief.Shared.Models.UI;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using Radzen;
using EBrief.Shared.Models.Shared;
using EBrief.Shared.Services.Search;

namespace EBrief.Pages;
public partial class CourtListPage : ICourtListPage
{
    [Inject] public IDataAccess DataAccess { get; set; } = default!;
    [Inject] public IFileService FileService { get; set; } = default!;
    [Inject] public AppState AppState { get; set; } = default!;
    [Inject] public HttpService HttpService { get; set; } = default!;
    public SearchService SearchService { get; set; } = default!;
    private CourtList CourtList { get; set; } = default!;
    public List<CourtSession> CourtSessions { get; set; } = [];
    private CourtCode CourtCode { get; set; } = default!;
    private DateTime CourtDate { get; set; } = default!;
    private int CourtRoom { get; set; } = default!;
    private bool IncludeDocuments { get; set; }
    private List<SearchResult> SearchResults { get; set; } = [];
    private ElementReference? _unsavedChangesDialog { get; set; }
    private ElementReference? _addCasefilesDialog { get; set; }
    private ElementReference? _searchDialog { get; set; }
    private string CasefilesToAdd { get; set; } = string.Empty;
    public Defendant? ActiveDefendant { get; set; }
    public event Func<Task>? OnDefendantChange;
    private string? _error;
    private string? _addCasefilesError;
    private bool _loading;
    private bool _loadingNewCasefiles;
    private readonly string _casefileSelected = "bg-blue text-text hover:bg-blue";
    private readonly string _casefileNotSelected = "hover:bg-slate-300";

    protected override async Task OnInitializedAsync()
    {
        await JSRuntime.InvokeVoidAsync("addSearchEventHandler", DotNetObjectReference.Create(this));
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
        SearchService = new SearchService(CourtList);

        AppState.CurrentCourtList = CourtList;
        _loading = false;
    }

    private async Task LoadCourtList(CourtCode courtCode, DateTime courtDate, int courtRoom)
    {
        var courtList = (await DataAccess.GetCourtList(courtCode, courtDate, courtRoom))?.ToUIModel();

        CourtList = courtList ?? throw new Exception("Failed to load court list.");
        CourtList.CourtCode = courtCode;
        CourtList.CourtDate = courtDate;
        CourtList.CourtRoom = courtRoom;
    }

    private List<CourtSession> GenerateCourtSessions()
    {
        // iterate over the list of defendants and group them by the appearance time of their first case file in the list
        var courtSessions = CourtList.Defendants.SelectMany(d => d.Casefiles)
            .GroupBy(cf => cf.Schedule.Last().HearingDate)
            .OrderBy(g => g.Key)
            .Select((g, i) => new CourtSession(i, g.Key))
            .ToList();

        foreach (var defendant in CourtList.Defendants)
        {
            var hearingTime = defendant.Casefiles.First().Schedule.Last().HearingDate;
            courtSessions.First(cs => cs.SittingTime.TimeOfDay == hearingTime.TimeOfDay).Defendants.Add(defendant);
        }

        courtSessions.ForEach(cs => cs.Defendants.Sort((d1, d2) => d1.ListStart.CompareTo(d2.ListStart)));

        return courtSessions;
    }

    private async Task OpenAddCasefilesDialog()
    {
        if (_addCasefilesDialog is not null)
        {
            _error = null;
            await JSRuntime.InvokeAsync<string>("openDialog", _addCasefilesDialog);
        }
    }

    private async Task CloseAddCasefilesDialog()
    {
        CasefilesToAdd = string.Empty;
        await JSRuntime.InvokeVoidAsync("closeDialog", _addCasefilesDialog);
    }

    private async Task AddCasefiles()
    {
        _loadingNewCasefiles = true;
        var casefileNumbers = CasefilesToAdd.Split(' ', '\n').Where(e => !string.IsNullOrWhiteSpace(e));
        var newCasefileNumbers = casefileNumbers.Except(CourtList.GetCasefiles().Select(cf => cf.CasefileNumber)).ToList();

        if (newCasefileNumbers.Count == 0)
        {
            _addCasefilesError = "All of those case files are in the list already";
            return;
        }

        try
        {
            _addCasefilesError = null;
            var newCasefileModels = await HttpService.GetCasefiles(newCasefileNumbers, CourtDate);
            await DataAccess.AddCasefiles(newCasefileModels, CourtList);
            var newCasefiles = newCasefileModels.ToUIModels();
            newCasefiles.AddReferenceToDefendants();
            CourtList.AddCasefiles(newCasefiles);
            UpdateCourtSessions(newCasefiles.Select(cf => cf.Defendant).ToList());

            CasefilesToAdd = string.Empty;
            await JSRuntime.InvokeVoidAsync("closeDialog", _addCasefilesDialog);
            _loadingNewCasefiles = false;
        }
        catch
        {
            _addCasefilesError = "Failed to add case files";
            _loadingNewCasefiles = false;
            return;
        }

    }

    private void UpdateCourtSessions(List<Defendant> defendants)
    {
        foreach (var defendant in defendants)
        {
            var courtSession = CourtSessions.FirstOrDefault(cs => cs.SittingTime == defendant.Casefiles.First().Schedule.Last().HearingDate);
            if (courtSession is null)
            {
                courtSession = new(CourtSessions.Count, defendant.Casefiles.First().Schedule.Last().HearingDate);
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
        if (ActiveDefendant.ActiveCasefile is null)
        {
            ActiveDefendant.ActiveCasefile = ActiveDefendant.Casefiles.First();
        }

        OnDefendantChange?.Invoke();
        StateHasChanged();
    }

    private void CasefileChanged(Casefile casefile)
    {
        if (ActiveDefendant is not null)
        {
            ActiveDefendant.ActiveCasefile = casefile;
        }
        ActivateDefendant(ActiveDefendant!);
    }

    private void SearchCasefiles(ChangeEventArgs e)
    {
        if (e.Value is not null)
        {
            SearchResults = SearchService.Find((string)e.Value);
        }
    }

    [JSInvokable]
    public async void OpenSearchDialog()
    {
        var casefiles = CourtList.GetCasefiles(10);
        SearchResults = casefiles.Select(cf => new SearchResult(cf, "")).ToList();
        await JSRuntime.InvokeVoidAsync("openDialog", _searchDialog);
        StateHasChanged();
    }

    [JSInvokable]
    public void CloseSearchDialog()
    {
        SearchResults.Clear();
    }

    public bool IsSelected(Defendant defendant) => ActiveDefendant?.Id == defendant.Id;

    private bool IsSelected(Casefile casefile) => ActiveDefendant?.ActiveCasefile?.CasefileNumber == casefile.CasefileNumber;

    private void SaveCourtList()
    {
        DataAccess.UpdateCourtList(CourtList);

        foreach (var casefile in CourtList.GetCasefiles())
        {
            casefile.Notes.HasChanged = false;
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

        foreach (var casefile in CourtList.GetCasefiles())
        {
            if (casefile.Notes.HasChanged)
            {
                return true;
            }
        }

        return false;
    }

    private async Task RefreshData()
    {
        try
        {
            var updatedCasefiles = await HttpService.RefreshData(CourtList.GetCasefiles().Select(cf => cf.CasefileNumber));
            await DataAccess.UpdateCasefiles(updatedCasefiles);
            CourtList.UpdateCasefiles(updatedCasefiles);
            NavManager.Refresh();
        }
        catch (Exception e)
        {
            _error = e.Message;
            return;
        }
    }
}
