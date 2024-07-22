using EBrief.WebClient.Data;
using EBrief.Shared.Models;
using EBrief.Shared.Models.Data;
using EBrief.Shared.Models.UI;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using EBrief.Shared.Data;

namespace EBrief.WebClient.Pages;
public partial class CourtListPage : ICourtListPage
{
    [Inject]
    private IDataAccess LocalStorage { get; set; } = default!;
    public bool NewList { get; set; }
    private CourtList CourtList { get; set; } = default!;
    private CourtCode CourtCode { get; set; } = default!;
    private DateTime CourtDate { get; set; } = default!;
    private int CourtRoom { get; set; } = default!;
    private ElementReference? UnsavedChangesDialog { get; set; }
    private ElementReference? AddCaseFilesDialog { get; set; }
    private string CaseFilesToAdd { get; set; } = string.Empty;
    public Defendant? ActiveDefendant { get; set; }
    public event Func<Task>? OnDefendantChange;
    private string? _error;
    private string? _addCaseFilesError;
    private bool _loading;
    private bool _loadingNewCaseFiles;

    protected override async Task OnInitializedAsync()
    {
        _loading = true;
        var queries = QueryHelpers.ParseQuery(NavManager.ToAbsoluteUri(NavManager.Uri).Query);
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
        ActivateDefendant(CourtList.Defendants.First());
        _loading = false;
    }

    private async Task LoadCourtList(CourtCode courtCode, DateTime courtDate, int courtRoom)
    {
        var courtList = await LocalStorage.GetCourtList(courtCode, courtDate, courtRoom);

        if (courtList is null)
        {
            throw new Exception("Failed to load court list.");
        }

        CourtList = courtList.ToUIModel();
        CourtList.CourtCode = courtCode;
        CourtList.CourtDate = courtDate;
        CourtList.CourtRoom = courtRoom;
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
            var newCaseFiles = DummyData.GenerateCaseFiles(newCaseFileNumbers);
            CourtList.AddCaseFiles(newCaseFiles.ToUIModels());
            await LocalStorage.UpdateCourtList(CourtList);
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
        //if (UnsavedChanges())
        //{
        //    await JSRuntime.InvokeVoidAsync("openDialog", UnsavedChangesDialog);
        //}

        NavManager.NavigateTo("/EBrief");
    }

    private async Task CloseUnsavedChangesDialog()
    {
        await JSRuntime.InvokeVoidAsync("closeDialog", UnsavedChangesDialog);
    }
    private void ActivateDefendant(Defendant defendant)
    {
        ActiveDefendant = defendant;
        if (ActiveDefendant.ActiveCaseFile is null)
        {
            ActiveDefendant.ActiveCaseFile = ActiveDefendant.CaseFiles.First();
        }

        OnDefendantChange?.Invoke();
    }

    private void CaseFileChanged()
    {
        ActivateDefendant(ActiveDefendant!);
    }

    private void CaseFileChanged(CaseFile caseFile)
    {
        if (ActiveDefendant is not null)
        {
            ActiveDefendant.ActiveCaseFile = caseFile;
        }
        ActivateDefendant(ActiveDefendant!);
    }

    private string IsSelected(Defendant defendant)
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

    private async Task UpdateCourtList()
    {
        await LocalStorage.UpdateCourtList(CourtList);
    }

    bool ICourtListPage.IsSelected(Defendant defendant)
    {
        throw new NotImplementedException();
    }

    void ICourtListPage.ActivateDefendant(Defendant defendant)
    {
        throw new NotImplementedException();
    }

    //private bool UnsavedChanges()
    //{
    //    // Handles the case where something has gone wrong and the user wants to go back to the start
    //    if (_error is not null)
    //    {
    //        return false;
    //    }
    //    var courtList = DataAccess.GetCourtList(DataAccess.BuildKey(CourtCode, CourtDate, CourtRoom))!;

    //    foreach (var caseFile in CourtList.GetCaseFiles())
    //    {
    //        var caseFileModel = courtList.CaseFiles.First(cf => cf.CaseFileNumber == caseFile.CaseFileNumber);
    //        if (caseFileModel.Notes != caseFile.Notes)
    //        {
    //            return true;
    //        }
    //    }

    //    return false;
    //}

}
