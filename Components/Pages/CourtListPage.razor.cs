using CourtSystem.Data;
using CourtSystem.Helpers;
using CourtSystem.Models.UI;

namespace CourtSystem.Components.Pages;
public partial class CourtListPage {
    private CourtList CourtList { get; set; } = default!;
    public Defendant? ActiveDefendant { get; set; }
    private string? _error;
    private readonly CourtListDataAccess _dataAccess = new();

    protected override async Task OnInitializedAsync() {
        Console.WriteLine("Getting court list");
        var courtList = _dataAccess.GetCourtList()?.ToCourtList();

        if (courtList is null) {
            _error = "Failed to load the court list.";
            return;
        }
        CourtList = courtList;
        CourtList.GenerateInformations();
        CourtList.Defendants.Sort((a, b) => string.Compare(a.LastName, b.LastName, StringComparison.Ordinal));
        ActivateDefendant(CourtList.Defendants.First());

        await DownloadDocuments(courtList);
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

    private static async Task DownloadDocuments(CourtList courtList) {
        var client = new HttpClient();

        List<string> caseFileDocumentNames = [];
        List<string> occurrenceDocumentNames = [];

        foreach (var defendant in courtList.Defendants) {
            foreach (var caseFile in defendant.CaseFiles) {
                foreach (var caseFileDocument in caseFile.CaseFileDocuments) {
                    caseFileDocumentNames.Add(caseFileDocument.FileName);
                }

                foreach (var occurrenceDocument in caseFile.OccurrenceDocuments) {
                    occurrenceDocumentNames.Add(occurrenceDocument.FileName);
                }
            }
        }

        foreach (var document in caseFileDocumentNames) {
            await DownloadCaseFileDocument(document, client);
        }

        foreach (var document in occurrenceDocumentNames) {
            await DownloadEvidence(document, client);
        }
    }

    private static async Task DownloadCaseFileDocument(string fileName, HttpClient client) {
        var response = await client.GetAsync($"{AppConstants.ApiBaseUrl}/correspondence/?fileName={fileName}");
        var correspondenceDirectory = Path.Combine(FileSystem.AppDataDirectory, "correspondence");

        if (!Directory.Exists(correspondenceDirectory)) {
            Directory.CreateDirectory(correspondenceDirectory);
        }

        if (response.IsSuccessStatusCode) {
            var pdfStream = await response.Content.ReadAsStreamAsync();
            var memoryStream = new MemoryStream();
            await pdfStream.CopyToAsync(memoryStream);

            using var fileStream = new FileStream($"{correspondenceDirectory}/{fileName}", FileMode.Create, FileAccess.Write);
            fileStream.Write(memoryStream.ToArray());
        }
    }

    private static async Task DownloadEvidence(string fileName, HttpClient client) {
        var response = await client.GetAsync($"{AppConstants.ApiBaseUrl}/evidence/?fileName={fileName}");
        var evidenceDirectory = Path.Combine(FileSystem.AppDataDirectory, "evidence");

        if (!Directory.Exists(evidenceDirectory)) {
            Directory.CreateDirectory(evidenceDirectory);
        }

        if (response.IsSuccessStatusCode) {
            var pdfStream = await response.Content.ReadAsStreamAsync();
            var memoryStream = new MemoryStream();
            await pdfStream.CopyToAsync(memoryStream);

            using var fileStream = new FileStream($"{evidenceDirectory}/{fileName}", FileMode.Create, FileAccess.Write);
            fileStream.Write(memoryStream.ToArray());
        }
    }
}
