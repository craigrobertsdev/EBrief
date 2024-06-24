using EBrief.Data;
using EBrief.Helpers;
using EBrief.Models;
using EBrief.Models.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http;
using System.Net.Http.Json;

namespace EBrief.Pages;

public partial class Home
{
    [Inject] private IDataAccess DataAccess { get; set; } = default!;
    [Inject] private IFileService FileService { get; set; } = default!;
    public ElementReference? NewCourtListDialog { get; set; }
    public ElementReference? PreviousCourtListDialog { get; set; }
    public string CaseFileNumbers { get; set; } = string.Empty;
    private List<Court> Courts = [];
    private Court? SelectedCourt { get; set; }
    public DateTime? CourtDate { get; set; }
    public int? CourtRoom { get; set; }
    public bool IncludeDocuments { get; set; }
    private bool _loadingCourtList;
    public string? _error;
    private List<CourtListEntry>? PreviousCourtLists { get; set; }
    private CourtListEntry? SelectedCourtList { get; set; }

    protected override void OnInitialized()
    {
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
    }

    private async Task OpenNewCourtListDialog()
    {
        if (NewCourtListDialog is not null)
        {
            _error = null;
            await JSRuntime.InvokeVoidAsync("openDialog", NewCourtListDialog);
        }
    }

    private async Task LoadCourtListFromFile()
    {
        try
        {
            var courtEntry = await FileService.LoadCourtFile();
            if (courtEntry is null)
            {
                _error = "Failed to load court list.";
                return;
            }

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

    private void DeletePreviousCourtList()
    {
        if (SelectedCourtList is null || PreviousCourtLists is null)
        {
            return;
        }

        try
        {
            DataAccess.DeleteCourtList(new CourtListEntry(SelectedCourtList.CourtCode, SelectedCourtList.CourtDate, SelectedCourtList.CourtRoom));
        }
        catch (Exception e)
        {
            _error = e.InnerException?.Message ?? e.Message;
            return;
        }

        PreviousCourtLists.Remove(SelectedCourtList);
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
                await JSRuntime.InvokeVoidAsync("alert", "Failed to fetch court list.");
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

            courtList.CombineDefendantCaseFiles();

            try
            {
                await DataAccess.CreateCourtList(courtList);
            }
            catch (Exception e)
            {
                _error = e.InnerException?.Message ?? e.Message;
                return;
            }

            NavManager.NavigateTo($"/court-list/?newList=&courtCode={SelectedCourt.CourtCode}&courtRoom={CourtRoom}&courtDate={CourtDate}");
        }
        catch (Exception e)
        {
            _error = e.InnerException?.Message ?? e.Message;
        }

    }

    private async Task CloseLoadNewCourtListDialog()
    {
        if (NewCourtListDialog is not null)
        {
            await JSRuntime.InvokeVoidAsync("closeDialog", NewCourtListDialog);
        }
    }

    private async Task ClosePreviousCourtListDialog()
    {
        if (PreviousCourtListDialog is not null)
        {
            await JSRuntime.InvokeVoidAsync("closeDialog", PreviousCourtListDialog);
        }
    }

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

    record CourtListDto(IEnumerable<string> CaseFileNumbers, DateTime CourtDate);

}
