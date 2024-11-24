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
public partial class CourtListPage : ICourtListPage, IDisposable
{
    [Inject] public IDataAccess DataAccess { get; set; } = default!;
    [Inject] public IFileService FileService { get; set; } = default!;
    [Inject] public AppState AppState { get; set; } = default!;
    [Inject] public HttpService HttpService { get; set; } = default!;
    public SearchService SearchService { get; set; } = default!;
    private CourtList CourtList { get; set; } = default!;
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

        if (AppState.IsFirstLoad)
        {
            AppState.CourtSittings = await LoadCourtSittings();
        }

        ActivateDefendant(AppState.CourtSittings[0].Defendants.First());
        SearchService = new SearchService(CourtList);

        AppState.CurrentCourtList = CourtList;
        AppState.ApplicationLoaded();

        AppState.OnStateChanged += StateHasChanged;
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

    private async Task<List<CourtSitting>> LoadCourtSittings()
    {
        var courtSittings = await DataAccess.GetCourtSittings(new CourtListEntry(CourtCode, CourtDate, CourtRoom));
        return GenerateCourtSittings(courtSittings);
    }

    private List<CourtSitting> GenerateCourtSittings(List<CourtSitting> courtSittings)
    {
        if (courtSittings.Count == 0)
        {
            courtSittings = CourtList.Defendants.SelectMany(d => d.Casefiles)
                .GroupBy(cf => cf.Schedule.Last().HearingDate)
                .OrderBy(g => g.Key)
                .Select((g, i) => new CourtSitting(i, g.Key, CourtCode, CourtRoom))
                .ToList();
        }

        // iterate over the list of defendants and group them by the appearance time of their first case file in the list
        foreach (var defendant in CourtList.Defendants)
        {
            var hearingTime = defendant.Casefiles.First().Schedule.Last().HearingDate;
            var courtSitting = courtSittings.First(courtSittings => courtSittings.SittingTime.TimeOfDay == hearingTime.TimeOfDay);
            courtSitting.Defendants.Add(defendant);
        }

        courtSittings.ForEach(cs => cs.Defendants.Sort((d1, d2) => d1.ListStart.CompareTo(d2.ListStart)));
        courtSittings.Sort((a, b) => a.Id.CompareTo(b.Id));

        return courtSittings;
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
            _addCasefilesError = "You must include at least one unique, new casefile number";
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
            UpdateCourtSittings(newCasefiles.Select(cf => cf.Defendant));

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

    private async void UpdateCourtSittings(IEnumerable<Defendant> defendants)
    {
        foreach (var defendant in defendants)
        {
            var courtSession = AppState.CourtSittings.FirstOrDefault(cs => cs.SittingTime == defendant.Casefiles.First().Schedule.Last().HearingDate);
            if (courtSession is null)
            {
                courtSession = new AdditionsListCourtSitting(AppState.CourtSittings.Count, defendant.Casefiles.First().Schedule.Last().HearingDate, CourtCode, CourtRoom);
            }

            courtSession.Defendants.Add(defendant);
            AppState.CourtSittings.Add(courtSession);

            await DataAccess.UpdateCourtSittings(AppState.CourtSittings, new CourtListEntry(CourtCode, CourtDate, CourtRoom));

            OnCourtSessionAdded?.Invoke(courtSession);
        }
    }

    private async Task HandleReturnHome()
    {
        if (UnsavedChanges())
        {
            await JSRuntime.InvokeVoidAsync("openDialog", _unsavedChangesDialog);
        }
        else
        {
            await ReturnHome();
        }
    }

    private async Task SaveChanges()
    {
        await DataAccess.UpdateCourtList(CourtList);
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
        await JSRuntime.InvokeVoidAsync("closeDialog", _unsavedChangesDialog);
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

    public void Dispose()
    {
        AppState.OnStateChanged -= StateHasChanged;
    }
}
