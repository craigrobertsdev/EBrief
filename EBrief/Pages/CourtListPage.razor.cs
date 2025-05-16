using EBrief.Shared.Data;
using EBrief.Shared.Models.Data;
using EBrief.Shared.Services;
using EBrief.Shared.Models.UI;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using EBrief.Shared.Models.Shared;
using EBrief.Shared.Services.Search;

namespace EBrief.Pages;

public partial class CourtListPage : ICourtListPage, IDisposable
{
    [Inject] public IDataAccessFactory DataAccessFactory { get; set; } = null!;
    [Inject] public IFileServiceFactory FileServiceFactory { get; set; } = null!;
    [Inject] public AppState AppState { get; set; } = null!;
    [Inject] public HttpService HttpService { get; set; } = null!;
    public SearchService SearchService { get; set; } = null!;
    private CourtList CourtList { get; set; } = null!;
    private CourtCode CourtCode { get; set; } = default!;
    private DateTime CourtDate { get; set; } = default!;
    private int CourtRoom { get; set; } = 0!;
    private bool IncludeDocuments { get; set; }
    private List<SearchResult> SearchResults { get; set; } = [];
    private SearchResult? SelectedSearchResult { get; set; }
    private int SearchResultIndex { get; set; } = -1;
    private ElementReference? UnsavedChangesDialog { get; set; }
    private ElementReference? AddCasefilesDialog { get; set; }
    private ElementReference? SearchDialog { get; set; }
    private string CasefilesToAdd { get; set; } = string.Empty;
    public Defendant? ActiveDefendant { get; set; }
    public event Func<Task>? OnDefendantChanged;
    public event Action<CourtSitting>? OnCourtSessionAdded;
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

        if (AppState.IsFirstCourtListLoad)
        {
            AppState.CourtSittings = await LoadCourtSittings();
        }

        ActivateDefendant(AppState.CourtSittings[0].Defendants.First());
        SearchService = new SearchService(CourtList);

        AppState.CurrentCourtList = CourtList;
        AppState.CourtListLoaded();

        AppState.OnStateChanged += StateHasChanged;
        _loading = false;
    }

    private async Task LoadCourtList(CourtCode courtCode, DateTime courtDate, int courtRoom)
    {
        var dataAccess = DataAccessFactory.Create();
        var courtList = (await dataAccess.GetCourtList(courtCode, courtDate, courtRoom))?.ToUIModel();

        CourtList = courtList ?? throw new Exception("Failed to load court list.");
        CourtList.CourtCode = courtCode;
        CourtList.CourtDate = courtDate;
        CourtList.CourtRoom = courtRoom;
    }

    private async Task<List<CourtSitting>> LoadCourtSittings()
    {
        var dataAccess = DataAccessFactory.Create();
        var courtSittings = await dataAccess.GetCourtSittings(new CourtListEntry(CourtCode, CourtDate, CourtRoom));

        if (courtSittings.Count == 0)
        {
            courtSittings = CourtList.Defendants.SelectMany(d => d.Casefiles)
                .GroupBy(cf => cf.Schedule.Last().HearingDate)
                .OrderBy(g => g.Key)
                .Select((g, i) => new CourtSitting(i, g.Key, CourtCode, CourtRoom))
                .ToList();

            await dataAccess.SaveCourtSittings(courtSittings);
        }

        return GenerateCourtSittings(courtSittings);
    }

    private List<CourtSitting> GenerateCourtSittings(List<CourtSitting> courtSittings)
    {
        // iterate over the list of defendants and group them by the appearance time of their first case file in the list
        foreach (var defendant in CourtList.Defendants)
        {
            var hearingTime = defendant.Casefiles.First().Schedule.Last().HearingDate;
            var courtSitting =
                courtSittings.FirstOrDefault(sittings => sittings.SittingTime.TimeOfDay == hearingTime.TimeOfDay);
            courtSitting?.Defendants.Add(defendant);
        }

        courtSittings.ForEach(cs => cs.Defendants.Sort((d1, d2) => d1.ListStart.CompareTo(d2.ListStart)));
        courtSittings.Sort((a, b) => a.Id.CompareTo(b.Id));

        return courtSittings;
    }

    private async Task OpenAddCasefilesDialog()
    {
        if (AddCasefilesDialog is not null)
        {
            _error = null;
            await JSRuntime.InvokeAsync<string>("openDialog", AddCasefilesDialog);
        }
    }

    private async Task CloseAddCasefilesDialog()
    {
        CasefilesToAdd = string.Empty;
        await JSRuntime.InvokeVoidAsync("closeDialog", AddCasefilesDialog);
    }

    private async Task AddCasefiles()
    {
        _loadingNewCasefiles = true;
        var casefileNumbers = CasefilesToAdd.Split(' ', '\n').Where(e => !string.IsNullOrWhiteSpace(e));
        var newCasefileNumbers =
            casefileNumbers.Except(CourtList.GetCasefiles().Select(cf => cf.CasefileNumber)).ToList();

        if (newCasefileNumbers.Count == 0)
        {
            _addCasefilesError = "You must include at least one unique, new casefile number";
            return;
        }

        try
        {
            _addCasefilesError = null;

            var newCasefileModels = await HttpService.GetCasefiles(newCasefileNumbers, CourtDate);

            var dataAccess = DataAccessFactory.Create();
            await dataAccess.AddCasefiles(newCasefileModels, CourtList);

            var newCasefiles = newCasefileModels.ToUIModels();
            newCasefiles.AddReferenceToDefendants();

            CourtList.AddCasefiles(newCasefiles);
            UpdateCourtSittings(newCasefiles.Select(cf => cf.Defendant));

            CasefilesToAdd = string.Empty;

            await JSRuntime.InvokeVoidAsync("closeDialog", AddCasefilesDialog);

            _loadingNewCasefiles = false;
        }
        catch
        {
            _addCasefilesError = "Failed to add case files";
            _loadingNewCasefiles = false;
            return;
        }
    }

    private async void UpdateCourtSittings(IEnumerable<Defendant> defendants)
    {
        try
        {
            foreach (var defendant in defendants)
            {
                var courtSession = AppState.CourtSittings.FirstOrDefault(cs =>
                    cs.SittingTime == defendant.Casefiles.First().Schedule.Last().HearingDate);
                if (courtSession is null)
                {
                    courtSession = new AdditionsListCourtSitting(AppState.CourtSittings.Count,
                        defendant.Casefiles.First().Schedule.Last().HearingDate, CourtCode, CourtRoom);
                }

                courtSession.Defendants.Add(defendant);
                AppState.AddCourtSitting(courtSession);

                var dataAccess = DataAccessFactory.Create();
                await dataAccess.UpdateCourtSittings(AppState.CourtSittings,
                    new CourtListEntry(CourtCode, CourtDate, CourtRoom));

                OnCourtSessionAdded?.Invoke(courtSession);
            }
        }
        catch (Exception e)
        {
            _error = e.Message;
        }
    }

    private async Task HandleReturnHome()
    {
        if (UnsavedChanges())
        {
            await JSRuntime.InvokeVoidAsync("openDialog", UnsavedChangesDialog);
        }
        else
        {
            await ReturnHome();
        }
    }

    private async Task SaveChanges()
    {
        var dataAccess = DataAccessFactory.Create();
        await dataAccess.UpdateCourtList(CourtList);
        await ReturnHome();
    }

    private async Task ReturnHome()
    {
        await JSRuntime.InvokeVoidAsync("removeSearchEventHandler");
        AppState.Clear();
        NavManager.NavigateTo("/");
    }

    private async Task CloseUnsavedChangesDialog()
    {
        await JSRuntime.InvokeVoidAsync("closeDialog", UnsavedChangesDialog);
    }

    public void ActivateDefendant(Defendant defendant)
    {
        ActiveDefendant = defendant;
        if (ActiveDefendant.ActiveCasefile is null)
        {
            ActiveDefendant.ActiveCasefile = ActiveDefendant.Casefiles.First();
        }

        OnDefendantChanged?.Invoke();
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
            SelectedSearchResult = SearchResults.FirstOrDefault();
        }
    }

    [JSInvokable]
    public async void OpenSearchDialog()
    {
        try
        {
            var casefiles = CourtList.GetCasefiles(10);
            SearchResults = casefiles.Select(cf => new SearchResult(cf, "")).ToList();
            SelectedSearchResult = SearchResults.FirstOrDefault();
            SearchResultIndex = 0;
            
            await JSRuntime.InvokeVoidAsync("openDialog", SearchDialog);
            StateHasChanged();
        }
        catch (Exception e)
        {
            _error = "Failed to load search window";
        }
    }

    [JSInvokable]
    public void SelectNextSearchResult()
    {
        if (SelectedSearchResult is null && SearchResults.Count > 0)
        {
            SelectedSearchResult = SearchResults.First();
            return;
        }

        if (SearchResultIndex >= SearchResults.Count - 1)
        {
            SelectedSearchResult = SearchResults[0];
            SearchResultIndex = 0;
        }
        else
        {
            SearchResultIndex++;
            SelectedSearchResult = SearchResults[SearchResultIndex];
        }

        StateHasChanged();
    }

    [JSInvokable]
    public void SelectPreviousSearchResult()
    {
        if (SelectedSearchResult is null && SearchResults.Count > 0)
        {
            SelectedSearchResult = SearchResults.Last();
            return;
        }

        if (SearchResultIndex <= 0)
        {
            SelectedSearchResult = SearchResults[^1];
            SearchResultIndex = SearchResults.Count - 1;
        }
        else
        {
            SearchResultIndex--;
            SelectedSearchResult = SearchResults[SearchResultIndex];
        }

        StateHasChanged();
    }

    [JSInvokable]
    public void ClearSearchResults()
    {
        
        SearchResults.Clear();
    }

    private async Task CloseSearchDialog()
    {
        SelectedSearchResult = null;
        SearchResultIndex = 0;
        ClearSearchResults();
        await JSRuntime.InvokeVoidAsync("clearSearchText");
        await JSRuntime.InvokeVoidAsync("closeDialog", SearchDialog);
    }

    public bool IsSelected(Defendant defendant) => ActiveDefendant?.Id == defendant.Id;

    private bool IsSelected(Casefile casefile) =>
        ActiveDefendant?.ActiveCasefile?.CasefileNumber == casefile.CasefileNumber;

    private bool IsSelected(SearchResult result) =>
        result.Casefile.CasefileNumber == SelectedSearchResult?.Casefile.CasefileNumber;

    private void SaveCourtList()
    {
        var dataAccess = DataAccessFactory.Create();
        dataAccess.UpdateCourtList(CourtList);

        foreach (var casefile in CourtList.GetCasefiles())
        {
            casefile.Notes.HasChanged = false;
        }
    }

    private async Task ExportCourtList()
    {
        var fileService = FileServiceFactory.Create();
        await fileService.SaveFile(CourtList);
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
            var updatedCasefiles =
                await HttpService.RefreshData(CourtList.GetCasefiles().Select(cf => cf.CasefileNumber));
            var dataAccess = DataAccessFactory.Create();
            await dataAccess.UpdateCasefiles(updatedCasefiles);
            CourtList.UpdateCasefiles(updatedCasefiles);
            NavManager.Refresh();
        }
        catch (Exception e)
        {
            _error = e.Message;
        }
    }

    public void Dispose()
    {
        AppState.OnStateChanged -= StateHasChanged;
    }

    private async Task SelectSearchResult(Casefile casefile)
    {
        ActiveDefendant = casefile.Defendant;
        ActiveDefendant.ActiveCasefile = casefile;

        await CloseSearchDialog();
    }

    [JSInvokable]
    public async Task SelectSearchResult()
    {
        if (SelectedSearchResult is null)
        {
            return;
        }

        ActiveDefendant = SelectedSearchResult.Casefile.Defendant;
        ActiveDefendant.ActiveCasefile = SelectedSearchResult.Casefile;

        await CloseSearchDialog();
        StateHasChanged();
    }
}