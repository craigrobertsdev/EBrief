using EBrief.Shared.Services.Search;

namespace EBrief.Shared.Models.UI;
public class Casefile
{
    public string CasefileNumber { get; set; } = string.Empty;
    public Defendant Defendant { get; set; } = default!;
    public string? CourtFileNumber { get; set; } = string.Empty;
    public List<HearingEntry> Schedule { get; set; } = [];
    public List<CasefileEnquiryLogEntry> Cfel { get; set; } = [];
    public string FactsOfCharge { get; set; } = default!;
    public Information Information { get; set; } = default!;
    public TimeSpan? TimeInCustody { get; set; }
    public List<Charge> Charges { get; set; } = [];
    public List<Document> CasefileDocuments { get; set; } = [];
    public Document? SelectedCasefileDocument { get; set; } = default!;
    public List<Document> OccurrenceDocuments { get; set; } = [];
    public bool EvidenceLoaded { get; set; }
    public bool CasefileDocumentsLoaded { get; set; }
    public Document? SelectedOccurrenceDocument { get; set; } = default!;
    public CasefileNote Notes { get; set; } = default!;
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

    public void Update(Casefile casefile)
    {
        Schedule = casefile.Schedule;
        Cfel = casefile.Cfel;
        FactsOfCharge = casefile.FactsOfCharge;
        Information = casefile.Information;
        TimeInCustody = casefile.TimeInCustody;
        Charges = casefile.Charges;
        CasefileDocuments = casefile.CasefileDocuments;
        OccurrenceDocuments = casefile.OccurrenceDocuments;

        Cfel.Sort((x, y) => y.EntryDate.CompareTo(x.EntryDate));
    }
}

public static class CasefileExtensions
{
    public static void AddReferenceToDefendants(this List<Casefile> casefiles) => casefiles.ForEach(cf => cf.Defendant.Casefiles.Add(cf));
}
