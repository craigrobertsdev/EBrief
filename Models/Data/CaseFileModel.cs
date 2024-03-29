using CourtSystem.Models.UI;
using System.ComponentModel.DataAnnotations;

namespace CourtSystem.Models.Data;
public class CaseFileModel {
    [Key]
    public string CaseFileNumber { get; set; } = string.Empty;
    public DefendantModel Defendant { get; set; } = default!;
    [MaxLength(20)]
    public string? CourtFileNumber { get; set; } = string.Empty;
    public List<HearingEntryModel> PreviousHearings { get; set; } = [];
    public List<CaseFileEnquiryLogModel> CfelEntries { get; set; } = [];
    public string FactsOfCharge { get; set; } = default!;
    public InformationModel? Information { get; set; }
    public List<ChargeModel> Charges { get; set; } = [];
    public List<CaseFileDocumentModel> CaseFileDocuments { get; set; } = [];
    public List<OccurrenceDocumentModel> OccurrenceDocuments { get; set; } = [];
    public string Notes { get; set; } = string.Empty;

    public CaseFile ToUIModel() {
        return new CaseFile {
            CaseFileNumber = CaseFileNumber,
            Defendant = Defendant.ToUIModel(),
            CourtFileNumber = CourtFileNumber,
            PreviousHearings = PreviousHearings.Select(hearing => hearing.ToUIModel()).ToList(),
            CfelEntries = CfelEntries.Select(cfel => cfel.ToUIModel()).ToList(),
            FactsOfCharge = FactsOfCharge,
            Charges = Charges.Select(charge => charge.ToUIModel()).ToList(),
            CaseFileDocuments = CaseFileDocuments.Select(doc => doc.ToUIModel()).ToList(),
            OccurrenceDocuments = OccurrenceDocuments.Select(doc => doc.ToUIModel()).ToList(),
            Notes = Notes
        };
    }
}
