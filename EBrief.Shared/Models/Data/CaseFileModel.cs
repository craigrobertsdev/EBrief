using EBrief.Shared.Models.UI;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EBrief.Shared.Models.Data;
public class CaseFileModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [JsonIgnore]
    public Guid Id { get; set; }
    public CourtListModel CourtList { get; set; } = default!;
    public Guid CourtListId { get; set; }
    public string CaseFileNumber { get; set; } = string.Empty;
    public DefendantModel Defendant { get; set; } = default!;
    [MaxLength(20)]
    public string? CourtFileNumber { get; set; } = string.Empty;
    public int ListNumber { get; set; }
    public string ListingType { get; set; } = string.Empty;
    public List<HearingEntryModel> Schedule { get; set; } = [];
    public List<CaseFileEnquiryLogEntryModel> CfelEntries { get; set; } = [];
    public string FactsOfCharge { get; set; } = default!;
    public InformationModel? Information { get; set; }
    public TimeSpan? TimeInCustody { get; set; }
    public List<ChargeModel> Charges { get; set; } = [];
    public List<DocumentModel> Documents { get; set; } = [];
    public bool DocumentsLoaded { get; set; }
    public string Notes { get; set; } = string.Empty;

    public CaseFile ToUIModel()
    {
        return new CaseFile
        {
            CaseFileNumber = CaseFileNumber,
            Defendant = Defendant.ToUIModel(),
            CourtFileNumber = CourtFileNumber,
            Schedule = Schedule.Select(hearing => hearing.ToUIModel()).ToList(),
            Cfel = CfelEntries.Select(cfel => cfel.ToUIModel()).ToList(),
            FactsOfCharge = FactsOfCharge,
            TimeInCustody = TimeInCustody,
            Charges = Charges.Select(charge => charge.ToUIModel()).ToList(),
            CaseFileDocuments = Documents.Where(doc => doc.DocumentType == DocumentType.CaseFile).Select(doc => doc.ToUIModel()).ToList(),
            OccurrenceDocuments = Documents.Where(doc => doc.DocumentType == DocumentType.Occurrence).Select(doc => doc.ToUIModel()).ToList(),
            DocumentsLoaded = DocumentsLoaded,
            Notes = new() { Text = Notes, HasChanged = false }
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
