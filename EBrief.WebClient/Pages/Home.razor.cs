using EBrief.WebClient.Data;
using EBrief.Shared.Models.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using EBrief.Shared.Data;
using EBrief.Shared.Models.Shared;

namespace EBrief.WebClient.Pages;

public partial class Home
{
    [Inject]
    public IDataAccess DataAccess { get; set; } = default!;
    public ElementReference? NewCourtListDialog { get; set; }
    public ElementReference? PreviousCourtListDialog { get; set; }
    private ElementReference? ConfirmDialog { get; set; }
    public string CaseFileNumbers { get; set; } = string.Empty;
    private Court? SelectedCourt { get; set; }
    private List<Court> Courts = [];
    public DateTime? CourtDate { get; set; }
    public int? CourtRoom { get; set; }
    public string? _error;
    private bool _loadingCourtList;
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

    private async Task OpenConfirmDialog()
    {
        if (ConfirmDialog is not null)
        {
            _error = null;
            await JSRuntime.InvokeVoidAsync("openDialog", ConfirmDialog);
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

        NavManager.NavigateTo($"/EBrief/court-list?courtCode={SelectedCourtList.CourtCode}&courtDate={SelectedCourtList.CourtDate}&courtRoom={SelectedCourtList.CourtRoom}");
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
            _error = "Please select a court code.";
            return;
        }

        if (CourtRoom is null)
        {
            _error = "Please select a court room.";
            return;
        }

        // this should only check whether the previous court list exists, not retrieve the whole list
        // the next call should save the court list to localStorage then the following call should 
        // navigate to the court list page
        var courtListExists = await DataAccess.CheckCourtListExists(new CourtListEntry(SelectedCourt.CourtCode, CourtDate.Value, CourtRoom!.Value));
        if (courtListExists)
        {
           _error = "Court list already exists.";
            return;
        }

        var caseFileNumbers = CaseFileNumbers.Split(' ', '\n').Where(e => !string.IsNullOrWhiteSpace(e));
        try
        {
            var caseFiles = DummyData.GenerateCaseFiles(caseFileNumbers, CourtDate.Value);
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
                CourtRoom = CourtRoom!.Value
            };

            courtList.CombineAndSortDefendantCaseFiles();

            try
            {
                await DataAccess.CreateCourtList(courtList);
            }
            catch (Exception e)
            {
                _error = e.InnerException?.Message ?? e.Message;
                return;
            }

            NavManager.NavigateTo($"/EBrief/court-list/?newList=true&courtCode={SelectedCourt.CourtCode}&courtRoom={CourtRoom}&courtDate={CourtDate}");
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

    private void HandleSelectCourtDate(ChangeEventArgs e)
    {
        if (e.Value is null)
        {
            return;
        }
        try
        {
            CourtDate = DateTime.Parse(e.Value.ToString()!);
        }
        catch (Exception)
        {
            CourtDate = null;
        }
    }

    private void HandleSelectCourt(ChangeEventArgs e)
    {
        if (e.Value is null)
        {
            return;
        }
        _error = null;
        var courtCode = Enum.Parse<CourtCode>(e.Value.ToString()!);
        SelectedCourt = Courts.FirstOrDefault(e => e.CourtCode == courtCode);
    }

    private void HandleSelectCourtRoom(ChangeEventArgs e)
    {
        if (e.Value is null)
        {
            return;
        }
        _error = null;
        CourtRoom = int.Parse(e.Value.ToString()!);
    }

    private List<int> CourtRooms = [2, 3, 12, 15, 17, 18, 19, 20, 22, 23, 24];

    class Court
    {
        public CourtCode CourtCode { get; set; }
        public List<int> CourtRooms { get; set; } = [];
    }
}
