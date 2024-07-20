using EBrief.Data;
using EBrief.Helpers;
using EBrief.Models;
using EBrief.Models.Data;
using EBrief.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace EBrief.Pages;

public partial class Home
{
    [Inject] private AppState AppState { get; set; } = default!;
    [Inject] private IDataAccess DataAccess { get; set; } = default!;
    [Inject] private IFileService FileService { get; set; } = default!;
    private ElementReference? NewCourtListDialog { get; set; }
    private ElementReference? PreviousCourtListDialog { get; set; }
    private ElementReference? ConfirmDialog { get; set; }
    private string CaseFileNumbers { get; set; } = string.Empty;
    private List<Court> Courts = [];
    private Court? SelectedCourt { get; set; }
    public DateTime? CourtDate { get; set; }
    private int? CourtRoom { get; set; }
    private bool IncludeDocuments { get; set; }
    private bool _loadingCourtList;
    private string? _error;
    private List<CourtListEntry>? PreviousCourtLists { get; set; }
    private CourtListEntry? SelectedCourtList { get; set; }

    protected override async Task OnInitializedAsync()
    {
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

    private async Task OpenNewCourtListDialog()
    {
        if (NewCourtListDialog is not null)
        {
            _error = null;
            var obj = DotNetObjectReference.Create(this);
            await JSRuntime.InvokeVoidAsync("openDialog", NewCourtListDialog, obj);
        }
    }

    private async Task OpenConfirmDialog()
    {
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

            NavManager.NavigateTo($"/court-list?courtCode={courtEntry.CourtCode}&courtDate={courtEntry.CourtDate}&courtRoom={courtEntry.CourtRoom}");
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
            NavManager.NavigateTo($"/court-list?courtCode={courtEntry.CourtCode}&courtDate={courtEntry.CourtDate}&courtRoom={courtEntry.CourtRoom}");
        }
        catch (Exception e)
        {
            _error = e.Message;
        }
    }

    private async Task OpenPreviousCourtListDialog()
    {
        PreviousCourtLists = await DataAccess.GetSavedCourtLists();
        SelectedCourtList = PreviousCourtLists.FirstOrDefault();

        if (PreviousCourtListDialog is not null)
        {
            _error = null;
            await JSRuntime.InvokeVoidAsync("openDialog", PreviousCourtListDialog);
        }
    }

    private void LoadPreviousCourtList()
    {
        if (SelectedCourtList is null)
        {
            _error = "Please select a court list.";
            return;
        }

        NavManager.NavigateTo($"/court-list?courtCode={SelectedCourtList.CourtCode}&courtDate={SelectedCourtList.CourtDate}&courtRoom={SelectedCourtList.CourtRoom}");
    }

    private async Task DeletePreviousCourtList(bool confirmDeletion)
    {
        await CloseConfirmDialog();
        if (!confirmDeletion)
        {
            return;
        }
        if (SelectedCourtList is null || PreviousCourtLists is null)
        {
            return;
        }

        try
        {
            await DataAccess.DeleteCourtList(new CourtListEntry(SelectedCourtList.CourtCode, SelectedCourtList.CourtDate, SelectedCourtList.CourtRoom));
        }
        catch (Exception e)
        {
            _error = e.InnerException?.Message ?? e.Message;
            return;
        }

        PreviousCourtLists.Remove(SelectedCourtList);
        SelectedCourtList = PreviousCourtLists.FirstOrDefault();
    }

    private async Task FetchCourtList()
    {
        _error = null;
        if (CourtDate is null)
        {
            _error = "Please select a court date.";
            return;
        }

        if (SelectedCourt is null)
        {
            _error = "Please select a court.";
            return;
        }

        if (CourtRoom is null)
        {
            _error = "Please select a court room.";
            return;
        }

        _loadingCourtList = true;
        var listAlreadyExists = await DataAccess.CheckCourtListExists(new CourtListEntry(SelectedCourt.CourtCode, CourtDate.Value, CourtRoom.Value));
        if (listAlreadyExists)
        {
            _error = "A court list for this date and location already exists";
            return;
        }

        var client = new HttpClient();
        var caseFileNumbers = CaseFileNumbers.Split(' ', '\n').Where(e => !string.IsNullOrWhiteSpace(e));
        try
        {
            var body = new CourtListDto(caseFileNumbers, CourtDate.Value);
            var response = await client.PostAsJsonAsync($"{AppConstants.ApiBaseUrl}/generate-case-files", body);
            if (!response.IsSuccessStatusCode)
            {
                _error = "Failed to connect to the server";
                return;
            }

            var caseFiles = await response.Content.ReadFromJsonAsync<List<CaseFileModel>>();
            if (caseFiles is null)
            {
                _error = "Failed to fetch court list.";
                return;
            }

            var courtList = new CourtListModel
            {
                CaseFiles = caseFiles,
                CourtCode = SelectedCourt.CourtCode,
                CourtDate = CourtDate.Value,
                CourtRoom = CourtRoom.Value
            };

            try
            {
                await DataAccess.CreateCourtList(courtList);
            }
            catch (Exception e)
            {
                _error = e.InnerException?.Message ?? e.Message;
                // not put in a finally block as the button flashes back to being enabled before the page changes
                _loadingCourtList = false;
                return;
            }

            NavManager.NavigateTo($"/court-list/?newList=&courtCode={SelectedCourt.CourtCode}&courtRoom={CourtRoom}&courtDate={CourtDate}");
        }
        catch (HttpRequestException e)
        {
            _error = "Failed to connect to the server";
            _loadingCourtList = false;
        }
        catch (Exception e)
        {
            _error = e.InnerException?.Message ?? e.Message;
            _loadingCourtList = false;
        }

    }

    private async Task CloseLoadNewCourtListDialog()
    {
        await JSRuntime.InvokeVoidAsync("closeDialog", NewCourtListDialog);
    }

    private async Task ClosePreviousCourtListDialog() =>
        await JSRuntime.InvokeVoidAsync("closeDialog", PreviousCourtListDialog);

    private async Task CloseConfirmDialog() => await JSRuntime.InvokeVoidAsync("closeDialog", ConfirmDialog);
    private void HandleSelectCourtListEntry(CourtListEntry courtListEntry)
    {
        if (SelectedCourtList == courtListEntry)
        {
            SelectedCourtList = null;
            return;
        }

        SelectedCourtList = courtListEntry;
    }

    private void HandleSelectCourt(ChangeEventArgs e)
    {
        if (e.Value is null)
        {
            return;
        }
        var courtCode = Enum.Parse<CourtCode>(e.Value.ToString()!);
        SelectedCourt = Courts.FirstOrDefault(e => e.CourtCode == courtCode);
    }

    private void HandleSelectCourtRoom(ChangeEventArgs e)
    {
        if (e.Value is null)
        {
            return;
        }
        CourtRoom = int.Parse(e.Value.ToString()!);
    }

    class Court
    {
        public CourtCode CourtCode { get; set; }
        public List<int> CourtRooms { get; set; } = [];
    }
}
