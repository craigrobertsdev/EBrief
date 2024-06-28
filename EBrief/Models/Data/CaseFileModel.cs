using EBrief.Models.UI;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EBrief.Models.Data;
public class CaseFileModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public CourtListModel CourtList { get; set; } = default!;
    public Guid CourtListId { get; set; }
    public string CaseFileNumber { get; set; } = string.Empty;
    public DefendantModel Defendant { get; set; } = default!;
    [MaxLength(20)]
    public string? CourtFileNumber { get; set; } = string.Empty;
    public List<HearingEntryModel> Schedule { get; set; } = [];
    public List<CaseFileEnquiryLogModel> CfelEntries { get; set; } = [];
    public string FactsOfCharge { get; set; } = default!;
    public InformationModel? Information { get; set; }
    public TimeSpan? TimeInCustody { get; set; }
    public List<ChargeModel> Charges { get; set; } = [];
    public List<CaseFileDocumentModel> CaseFileDocuments { get; set; } = [];
    public List<OccurrenceDocumentModel> OccurrenceDocuments { get; set; } = [];
    public string Notes { get; set; } = string.Empty;

    public CaseFile ToUIModel()
    {
        return new CaseFile
        {
            CaseFileNumber = CaseFileNumber,
            Defendant = Defendant.ToUIModel(),
            CourtFileNumber = CourtFileNumber,
            Schedule = Schedule.Select(hearing => hearing.ToUIModel()).ToList(),
            CfelEntries = CfelEntries.Select(cfel => cfel.ToUIModel()).ToList(),
            FactsOfCharge = FactsOfCharge,
            TimeInCustody = TimeInCustody,
            Charges = Charges.Select(charge => charge.ToUIModel()).ToList(),
            CaseFileDocuments = CaseFileDocuments.Select(doc => doc.ToUIModel()).ToList(),
            OccurrenceDocuments = OccurrenceDocuments.Select(doc => doc.ToUIModel()).ToList(),
            Notes = Notes
        };
    }
}

public static class CaseFileModelExtensions
{
    public static List<CaseFile> ToUIModels(this IEnumerable<CaseFileModel> caseFiles)
    {
        return caseFiles.Select(cf => cf.ToUIModel()).ToList();
    }
}
