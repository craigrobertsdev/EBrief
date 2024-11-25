using EBrief.Shared.Data;
using EBrief.Shared.Helpers;
using EBrief.Shared.Services;
using EBrief.Shared.Models.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.Json;
using System.IO;
using System.Net.Http;
using EBrief.Shared.Models.Shared;
using EBrief.Shared.Models.Validation;

namespace EBrief.Pages;

public partial class Home
{
    [Inject] private AppState AppState { get; set; } = default!;
    [Inject] private IDataAccess DataAccess { get; set; } = default!;
    [Inject] private IFileService FileService { get; set; } = default!;
    [Inject] private HttpService HttpService { get; set; } = default!;
    private ElementReference? NewCourtListDialog { get; set; }
    private ElementReference? PreviousCourtListDialog { get; set; }
    private ElementReference? ConfirmDialog { get; set; }
    private string? CasefileNumbers { get; set; }
    private List<Court> Courts = [];
    private string? SelectedFile { get; set; }
    private List<CourtListModel>? LandscapeList { get; set; }
    private CourtListBuilder CourtListBuilder { get; set; } = default!;
    private bool IncludeDocuments { get; set; }
    private bool _loadingCourtList;
    private bool EnterManually { get; set; }
    private string? _error;
    private string? _loadNewCourtListError;
    private bool _previousCourtListDialogIsOpen;
    private bool _deletingCourtList;

    private List<CourtListEntry>? PreviousCourtListEntries { get; set; }
    private CourtListEntry? SelectedCourtListEntry { get; set; }

    protected override async Task OnInitializedAsync()
    {
        CourtListBuilder = new CourtListBuilder();
        // This will be replaced when the application is in prod. Need to work out the actual court rooms
        // or create a generic list of court rooms up to 25 to cover everything. Need to add all the courts too
        Courts.Add(new Court
        {
            CourtCode = CourtCode.AMC,
            CourtRooms = [2, 3, 12, 15, 17, 18, 19, 20, 22, 23, 24]
        });

        Courts.Add(new Court
        {
            CourtCode = CourtCode.EMC,
            CourtRooms = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]
        });

        Courts.Add(new Court
        {
            CourtCode = CourtCode.CBMC,
            CourtRooms = [1, 2, 3, 4, 5]
        });

        Courts.Add(new Court
        {
            CourtCode = CourtCode.PAMC,
            CourtRooms = [1, 2, 3, 4, 5]
        });

        if (Environment.GetCommandLineArgs().Length > 1)
        {
            if (AppState.IsFirstLoad)
            {
                await LoadFromCommandLine();
            }
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var obj = DotNetObjectReference.Create(this);
            await JSRuntime.InvokeVoidAsync("addDeleteEventHandler", obj);
        }
    }

    private async Task OpenNewCourtListDialog()
    {
        if (NewCourtListDialog is not null)
        {
            _loadNewCourtListError = null;
            var obj = DotNetObjectReference.Create(this);
            await JSRuntime.InvokeVoidAsync("openDialog", NewCourtListDialog, obj);
        }
    }

    [JSInvokable]
    public async Task OpenConfirmDialog()
    {
        // this is to manage situations where the user presses delete while the PreviousCourtListDialog is not open.
        // it is controlled by a JS keydown event listener
        if (!_previousCourtListDialogIsOpen || PreviousCourtListEntries?.Count == 0)
        {
            return;
        }

        if (ConfirmDialog is not null)
        {
            _error = null;
            await JSRuntime.InvokeVoidAsync("openDialog", ConfirmDialog);
        }
    }

    private async Task LoadCourtListFromFile()
    {
        try
        {
            var (courtEntry, errorMessage) = await FileService.LoadCourtFile();
            if (courtEntry is null)
            {
                _error = errorMessage;
                return;
            }

            await NavigateToCourtList(courtEntry.CourtCode, courtEntry.CourtDate, courtEntry.CourtRoom);
        }
        catch (Exception e)
        {
            _error = e.Message;
        }
    }

    private async Task LoadFromCommandLine()
    {
        var args = Environment.GetCommandLineArgs();
        try
        {
            var file = File.ReadAllText(args[1]);
            var courtList = JsonSerializer.Deserialize<CourtListModel>(file);
            if (courtList is null)
            {
                _error = "Failed to load court list.";
                return;
            }

            var courtEntry = new CourtListEntry(courtList.CourtCode, courtList.CourtDate, courtList.CourtRoom);
            var previousCourtList = await DataAccess.CheckCourtListExists(courtEntry);
            if (!previousCourtList)
            {
                await DataAccess.CreateCourtList(courtList);
            }

            AppState.ApplicationLoaded();
            await NavigateToCourtList(courtEntry.CourtCode, courtEntry.CourtDate, courtEntry.CourtRoom);
        }
        catch (Exception e)
        {
            _error = e.Message;
        }
    }

    private async Task NavigateToCourtList(CourtCode courtCode, DateTime courtDate, int courtRoom, bool? includeDocuments = null)
    {
        await JSRuntime.InvokeVoidAsync("removeDeleteEventHandler");
        var incDocs = includeDocuments is not null ? $"&includeDocuments={includeDocuments!}" : string.Empty;
        NavManager.NavigateTo($"/court-list?courtCode={courtCode}&courtDate={courtDate}&courtRoom={courtRoom}{incDocs}");
    }

    private async Task OpenPreviousCourtListDialog()
    {
        _previousCourtListDialogIsOpen = true;
        PreviousCourtListEntries = await DataAccess.GetSavedCourtLists();
        SelectedCourtListEntry = PreviousCourtListEntries.FirstOrDefault();

        if (PreviousCourtListDialog is not null)
        {
            _error = null;
            await JSRuntime.InvokeVoidAsync("openDialog", PreviousCourtListDialog);
        }
    }

    private void LoadPreviousCourtList()
    {
        if (SelectedCourtListEntry is null)
        {
            _error = "Please select a court list.";
            return;
        }

        NavManager.NavigateTo($"/court-list?courtCode={SelectedCourtListEntry.CourtCode}&courtDate={SelectedCourtListEntry.CourtDate}&courtRoom={SelectedCourtListEntry.CourtRoom}");
    }

    private async Task DeletePreviousCourtList(bool confirmDeletion)
    {
        _deletingCourtList = true;
        await CloseConfirmDialog();

        if (!confirmDeletion || SelectedCourtListEntry is null || PreviousCourtListEntries is null)
        {
            return;
        }

        try
        {
            await DataAccess.DeleteCourtList(new CourtListEntry(SelectedCourtListEntry.CourtCode, SelectedCourtListEntry.CourtDate, SelectedCourtListEntry.CourtRoom));
            PreviousCourtListEntries.Remove(SelectedCourtListEntry);
            SelectedCourtListEntry = PreviousCourtListEntries.FirstOrDefault();
        }
        catch (Exception e)
        {
            _error = e.InnerException?.Message ?? e.Message;
        }
        finally
        {
            _deletingCourtList = false;
        }

    }

    private void ToggleManualCourtListEntry()
    {
        CourtListBuilder.CourtRoom = null;
        EnterManually = !EnterManually;
        StateHasChanged();
    }

    private async Task SelectFile()
    {
        _loadNewCourtListError = null;
        _loadingCourtList = true;
        try
        {

            SelectedFile = await FileService.SelectLandscapeList();
            if (SelectedFile is null)
            {
                LandscapeList = null;
                CourtListBuilder.Reset();
                _loadingCourtList = false;
                return;
            }

            var (landscapeList, error) = FileService.LoadLandscapeList(SelectedFile);
            if (error is not null)
            {
                _loadNewCourtListError = error;
                _loadingCourtList = false;
                return;
            }

            LandscapeList = landscapeList;
            CourtListBuilder.SetCourtDate(landscapeList![0].CourtDate);
            CourtListBuilder.SetCourtCode(landscapeList![0].CourtCode);
        }
        catch (Exception e)
        {
            _loadNewCourtListError = e.Message;
            _loadingCourtList = false;
        }
        finally
        {
            _loadingCourtList = false;
        }
    }

    private async Task SubmitNewCourtListForm()
    {
        _loadNewCourtListError = null;
        if (!CourtListBuilder.IsValid)
        {
            _loadNewCourtListError = "An error occurred. Please reload and try again.";
            return;
        }

        var courtList = CourtListBuilder.Build();
        await FetchCourtList(courtList);
    }

    private async Task FetchCourtList(CourtListModel courtList)
    {

        _loadingCourtList = true;
        var entry = new CourtListEntry(courtList.CourtCode, courtList.CourtDate, courtList.CourtRoom);
        var listAlreadyExists = await DataAccess.CheckCourtListExists(entry);
        if (listAlreadyExists)
        {
            _loadNewCourtListError = "A court list for this date and location already exists";
            _loadingCourtList = false;
            return;
        }

        try
        {
            var client = new HttpClient();

            IEnumerable<string> casefileNumbers;
            if (EnterManually)
            {
                casefileNumbers = CasefileNumbers!.Split(' ', '\n').Where(e => !string.IsNullOrWhiteSpace(e));
            }
            else
            {
                courtList.Casefiles = LandscapeList!.First(cl => cl.CourtRoom == courtList.CourtRoom).Casefiles;
                casefileNumbers = courtList.Casefiles.Select(cf => cf.CasefileNumber);
            }

            var body = new CourtListDto(casefileNumbers, courtList.CourtDate);
            var response = await client.PostAsJsonAsync($"{AppConstants.ApiBaseUrl}/generate-case-files", body);
            if (!response.IsSuccessStatusCode)
            {
                _loadNewCourtListError = "Failed to connect to the server";
                return;
            }

            var casefiles = await response.Content.ReadFromJsonAsync<List<CasefileModel>>();
            if (casefiles is null)
            {
                _loadNewCourtListError = "Failed to fetch court list.";
                return;
            }

            if (courtList.Casefiles.Count > 0)
            {
                courtList.CombineCasefiles(casefiles);
                courtList.CombineDefendantsWithServerResponse(casefiles);
            }
            else
            {
                courtList.Casefiles = casefiles;
                courtList.OrderAndAssignListingNumbers();
            }

            if (IncludeDocuments)
            {
                try
                {
                    await HttpService.DownloadDocuments(courtList);
                }

                catch (Exception e)
                {
                    _loadNewCourtListError = e.InnerException?.Message ?? e.Message;
                    _loadingCourtList = false;
                    return;
                }
            }

            await DataAccess.CreateCourtList(courtList);
            await NavigateToCourtList(courtList.CourtCode, courtList.CourtDate, courtList.CourtRoom, IncludeDocuments);
        }
        catch (HttpRequestException)
        {
            _loadNewCourtListError = "Failed to connect to the server";
            _loadingCourtList = false;
        }
        catch (Exception e)
        {
            _loadNewCourtListError = e.InnerException?.Message ?? e.Message;
            _loadingCourtList = false;
        }

    }

    private bool NewCourtListParametersAreValid()
    {

        if (!CourtListBuilder.IsValid)
        {
            return false;
        }

        if (EnterManually && string.IsNullOrEmpty(CasefileNumbers))
        {
            return false;
        }

        return true;
    }

    [JSInvokable]
    public async Task CloseLoadNewCourtListDialog()
    {
        ResetNewCourtListForm();
        await JSRuntime.InvokeVoidAsync("closeDialog", NewCourtListDialog);
        _loadingCourtList = false;
    }

    private void ResetNewCourtListForm()
    {
        SelectedFile = null;
        LandscapeList = null;
        CourtListBuilder.Reset();
        CasefileNumbers = null;
    }

    private async Task ClosePreviousCourtListDialog() =>
        await JSRuntime.InvokeVoidAsync("closeDialog", PreviousCourtListDialog);

    private async Task CloseConfirmDialog() => await JSRuntime.InvokeVoidAsync("closeDialog", ConfirmDialog);

    private void HandleSelectCourtRoom(ChangeEventArgs e)
    {
        if (e.Value is null)
        {
            return;
        }
        _loadNewCourtListError = null;
        var courtRoom = int.Parse((string)e.Value);
        CourtListBuilder.SetCourtRoom(courtRoom);
    }

    private void HandleSelectCourt(ChangeEventArgs e)
    {
        if (e.Value is null)
        {
            CourtListBuilder.SetCourtCode(null);
            return;
        }
        _loadNewCourtListError = null;

        try
        {
            var courtCode = Enum.Parse<CourtCode>((string)e.Value);
            CourtListBuilder.SetCourtCode(courtCode);
        }
        catch (Exception) { }
    }

    private void HandleEnterCourtRoom(ChangeEventArgs e)
    {
        _loadNewCourtListError = null;
        if (e.Value is null || (string)e.Value == string.Empty)
        {
            CourtListBuilder.SetCourtRoom(null);
            return;
        }
        _loadNewCourtListError = null;
        try
        {
            var courtRoom = int.Parse((string)e.Value);
            if (courtRoom < 1)
            {
                courtRoom = 1;
            }
            CourtListBuilder.SetCourtRoom(courtRoom);
        }
        catch (Exception)
        {
            CourtListBuilder.SetCourtRoom(null);
            _loadNewCourtListError = "A number must be entered in the court room field.";
        }
    }

    private void HandleIncludeDocuments(ChangeEventArgs e)
    {
        if (e.Value is null)
        {
            return;
        }

        IncludeDocuments = bool.Parse((string)e.Value);
    }

    private void HandleEnterCasefileNumbers(ChangeEventArgs e)
    {
        CasefileNumbers = (string?)e.Value;
    }

    private void HandleSelectCourtListEntry(CourtListEntry courtListEntry)
    {
        if (SelectedCourtListEntry == courtListEntry)
        {
            SelectedCourtListEntry = null;
            return;
        }

        SelectedCourtListEntry = courtListEntry;
    }

    class Court
    {
        public CourtCode CourtCode { get; set; }
        public List<int> CourtRooms { get; set; } = [];
    }
}
