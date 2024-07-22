namespace EBrief.Shared.Models.UI;
public class CaseFile
{
    public string CaseFileNumber { get; set; } = string.Empty;
    public Defendant Defendant { get; set; } = default!;
    public string? CourtFileNumber { get; set; } = string.Empty;
    public List<HearingEntry> Schedule { get; set; } = [];
    public List<CaseFileEnquiryLogEntry> Cfel { get; set; } = [];
    public string FactsOfCharge { get; set; } = default!;
    public Information Information { get; set; } = default!;
    public TimeSpan? TimeInCustody { get; set; }
    public List<Charge> Charges { get; set; } = [];
    public List<Document> CaseFileDocuments { get; set; } = [];
    public Document? SelectedCaseFileDocument { get; set; } = default!;
    public List<Document> OccurrenceDocuments { get; set; } = [];
    public bool DocumentsLoaded { get; set; }
    public Document? SelectedOccurrenceDocument { get; set; } = default!;
    public CaseFileNote Notes { get; set; } = default!;
    public string FollowUpText { get; set; } = string.Empty;

    public void GenerateInformationFromCharges()
    {
        List<InformationEntry> charges = [];
        foreach (var charge in Charges)
        {
            charges.Add(new InformationEntry(charge.Sequence, charge.ChargeWording));
        }

        Information = new Information
        {
            Charges = charges
        };
    }
}

public static class CaseFileExtensions
{
    public static void AddReferenceToDefendants(this List<CaseFile> caseFiles) => caseFiles.ForEach(cf => cf.Defendant.CaseFiles.Add(cf));
}
