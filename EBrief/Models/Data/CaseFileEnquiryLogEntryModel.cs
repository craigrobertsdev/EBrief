using EBrief.Models.UI;
using System.ComponentModel.DataAnnotations;

namespace EBrief.Models.Data;
public class CaseFileEnquiryLogEntryModel
{
    [Key]
    public int Id { get; set; }
    public string EntryText { get; set; } = string.Empty;
    public string EnteredBy { get; set; } = string.Empty;

    public DateTime EntryDate { get; set; }

    public CaseFileEnquiryLogEntry ToUIModel()
    {
        return new CaseFileEnquiryLogEntry(Id, EntryText, EnteredBy, EntryDate);
    }
}
