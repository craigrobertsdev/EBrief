using CourtSystem.Data;
using CourtSystem.Models.UI;
using Microsoft.AspNetCore.Components;

namespace CourtSystem.Components.Pages;
public partial class CourtListPage {
    private CourtList CourtList { get; set; } = default!;
    public Defendant? ActiveDefendant { get; set; }
    private string? _error { get; set; }
    private readonly CourtListDataAccess _dataAccess= new();

    protected override void OnInitialized() {
        var courtList = _dataAccess.GetCourtList()?.ToCourtList();   

        if (courtList is null) {
            _error = "Failed to load the court list.";
            return;
        }
        CourtList = courtList;
        CourtList.GenerateInformations();
    }

    private void ActivateDefendant(Defendant defendant) {
        ActiveDefendant = defendant;
        if (ActiveDefendant.ActiveCaseFile is null) {
            ActiveDefendant.ActiveCaseFile = ActiveDefendant.CaseFiles.First();
        }
    }

    private string IsSelected(Defendant defendant) {
        if (ActiveDefendant?.Id == defendant.Id) {
            return "!bg-sky-700";
        }

        return "hover:bg-gray-500";
    }

    private void SaveCourtList() {

    }
}
