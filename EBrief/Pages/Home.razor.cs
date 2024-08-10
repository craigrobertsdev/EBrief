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
    private ElementReference? NewCourtListDialog { get; set; }
    private ElementReference? PreviousCourtListDialog { get; set; }
    private ElementReference? ConfirmDialog { get; set; }
    private string? CaseFileNumbers { get; set; }
    private List<Court> Courts = [];
    private string? SelectedFile { get; set; }
    private List<CourtListModel>? LandscapeList { get; set; }
    private CourtListBuilder CourtListBuilder { get; set; } = default!;
    private bool IncludeDocuments { get; set; }
    private bool _loadingCourtList;
    private bool EnterManually { get; set; }
    private string? _error;
    private string? _loadNewCourtListError;
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

    private async Task OpenNewCourtListDialog()
    {
        if (NewCourtListDialog is not null)
        {
            _loadNewCourtListError = null;
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
        await CloseConfirmDialog();
        if (!confirmDeletion)
        {
            return;
        }
        if (SelectedCourtListEntry is null || PreviousCourtListEntries is null)
        {
            return;
        }

        try
        {
            await DataAccess.DeleteCourtList(new CourtListEntry(SelectedCourtListEntry.CourtCode, SelectedCourtListEntry.CourtDate, SelectedCourtListEntry.CourtRoom));
        }
        catch (Exception e)
        {
            _error = e.InnerException?.Message ?? e.Message;
            return;
        }

        PreviousCourtListEntries.Remove(SelectedCourtListEntry);
        SelectedCourtListEntry = PreviousCourtListEntries.FirstOrDefault();
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
        _loadingCourtList = false;
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

            IEnumerable<string> caseFileNumbers;
            if (EnterManually)
            {
                caseFileNumbers = CaseFileNumbers!.Split(' ', '\n').Where(e => !string.IsNullOrWhiteSpace(e));
            }
            else
            {
                courtList.CaseFiles = LandscapeList!.First(cl => cl.CourtRoom == courtList.CourtRoom).CaseFiles;
                caseFileNumbers = courtList.CaseFiles.Select(cf => cf.CaseFileNumber);
            }

            var body = new CourtListDto(caseFileNumbers, courtList.CourtDate);
            var response = await client.PostAsJsonAsync($"{AppConstants.ApiBaseUrl}/generate-case-files", body);
            if (!response.IsSuccessStatusCode)
            {
                _loadNewCourtListError = "Failed to connect to the server";
                return;
            }

            var caseFiles = await response.Content.ReadFromJsonAsync<List<CaseFileModel>>();
            if (caseFiles is null)
            {
                _loadNewCourtListError = "Failed to fetch court list.";
                return;
            }

            if (courtList.CaseFiles.Count > 0)
            {
                courtList.CombineCaseFiles(caseFiles);
                courtList.CombineDefendantsWithServerResponse(caseFiles);
            }
            else
            {
                courtList.CaseFiles = caseFiles;
                courtList.OrderAndAssignListingNumbers();
            }

            try
            {
                if (IncludeDocuments)
                {
                    await DownloadDocuments(courtList);
                }

                await DataAccess.CreateCourtList(courtList);
            }
            catch (Exception e)
            {
                _loadNewCourtListError = e.InnerException?.Message ?? e.Message;
                _loadingCourtList = false;
                return;
            }

            NavManager.NavigateTo($"/court-list/?newList=&courtCode={CourtListBuilder.CourtCode}&courtRoom={CourtListBuilder.CourtRoom}&courtDate={CourtListBuilder.CourtDate}&includeDocuments={IncludeDocuments}");
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

        if (EnterManually && string.IsNullOrEmpty(CaseFileNumbers))
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
        CaseFileNumbers = null;
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

    private void HandleEnterCaseFileNumbers(ChangeEventArgs e)
    {
        CaseFileNumbers = (string?)e.Value;
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

    private async Task DownloadDocuments(CourtListModel courtList)
    {
        var client = new HttpClient();
        FileService.CreateDocumentDirectory();

        foreach (var caseFile in courtList.CaseFiles)
        {
            foreach (var document in caseFile.Documents)
            {
                var endpoint = document.DocumentType == DocumentType.CaseFile ? "correspondence" : "evidence";
                await DownloadDocument(document, client, endpoint);
            }

            caseFile.DocumentsLoaded = true;
        }
    }

    private async Task DownloadDocument(DocumentModel document, HttpClient client, string endpoint)
    {
        var fileName = document.FileName;
        var response = await client.GetAsync($"{AppConstants.ApiBaseUrl}/{endpoint}/?fileName={fileName}");

        if (response.IsSuccessStatusCode)
        {
            var pdfStream = await response.Content.ReadAsStreamAsync();
            var ext = Path.GetExtension(fileName);
            var newFileName = Path.GetFileNameWithoutExtension(fileName) + $"-{Guid.NewGuid()}{ext}";
            await FileService.SaveDocument(pdfStream, newFileName);
            document.FileName = newFileName;
        }
    }

    class Court
    {
        public CourtCode CourtCode { get; set; }
        public List<int> CourtRooms { get; set; } = [];
    }
}
