using EBrief.Data;
using EBrief.Shared.Data;
using EBrief.Shared.Models.UI;
using EBrief.Shared.Models.Data;
using EBrief.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace EBrief.Pages;
public partial class TrialPage
{
    [Inject] private DataAccess DataAccess { get; set; } = default!;
    [Inject] private IFileService FileService { get; set; } = default!;
    [Inject] public AppState AppState { get; set; } = default!;
    [Inject] public HttpService HttpService { get; set; } = default!;
    private string? _error;
    private string? _addCasefilesError;
    private bool _loading;
    private bool _loadingNewCasefiles;
    private string CasefilesToAdd = string.Empty;
    private ElementReference? _unsavedChangesDialog { get; set; }
    private ElementReference? _addCasefilesDialog { get; set; }
    private CourtList CourtList { get; set; } = default!;

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
            var newCasefileModels = await HttpService.GetCasefiles(newCasefileNumbers, CourtList.CourtDate);
            await DataAccess.AddCasefiles(newCasefileModels, CourtList);
            var newCasefiles = newCasefileModels.ToUIModels();
            newCasefiles.AddReferenceToDefendants();
            CourtList.AddCasefiles(newCasefiles);

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

    private async Task CloseUnsavedChangesDialog() {
        await JSRuntime.InvokeVoidAsync("closeDialog", _unsavedChangesDialog);
    }
}
