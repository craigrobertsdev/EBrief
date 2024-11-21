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
    private string? _addCaseFilesError;
    private bool _loading;
    private bool _loadingNewCaseFiles;
    private string CaseFilesToAdd = string.Empty;
    private ElementReference? _unsavedChangesDialog { get; set; }
    private ElementReference? _addCaseFilesDialog { get; set; }
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

        foreach (var caseFile in CourtList.GetCaseFiles())
        {
            caseFile.Notes.HasChanged = false;
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

        foreach (var caseFile in CourtList.GetCaseFiles())
        {
            if (caseFile.Notes.HasChanged)
            {
                return true;
            }
        }

        return false;
    }

    private async Task OpenAddCaseFilesDialog()
    {
        if (_addCaseFilesDialog is not null)
        {
            _error = null;
            await JSRuntime.InvokeAsync<string>("openDialog", _addCaseFilesDialog);
        }
    }

    private async Task CloseAddCaseFilesDialog()
    {
        CaseFilesToAdd = string.Empty;
        await JSRuntime.InvokeVoidAsync("closeDialog", _addCaseFilesDialog);
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
            var newCaseFileModels = await HttpService.GetCaseFiles(newCaseFileNumbers, CourtList.CourtDate);
            await DataAccess.AddCaseFiles(newCaseFileModels, CourtList);
            var newCaseFiles = newCaseFileModels.ToUIModels();
            newCaseFiles.AddReferenceToDefendants();
            CourtList.AddCaseFiles(newCaseFiles);

            CaseFilesToAdd = string.Empty;
            await JSRuntime.InvokeVoidAsync("closeDialog", _addCaseFilesDialog);
            _loadingNewCaseFiles = false;
        }
        catch
        {
            _addCaseFilesError = "Failed to add case files";
            _loadingNewCaseFiles = false;
            return;
        }
    }

    private async Task CloseUnsavedChangesDialog() {
        await JSRuntime.InvokeVoidAsync("closeDialog", _unsavedChangesDialog);
    }
}
