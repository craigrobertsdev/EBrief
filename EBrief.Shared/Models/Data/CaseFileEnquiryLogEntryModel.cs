using EBrief.Shared.Models.UI;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EBrief.Shared.Models.Data;
public class CaseFileEnquiryLogEntryModel
{
    [Key]
    [JsonIgnore]
    public int Id { get; set; }
    public string EntryText { get; set; } = string.Empty;
    public string EnteredBy { get; set; } = string.Empty;

    public DateTime EntryDate { get; set; }

    public CasefileEnquiryLogEntry ToUIModel()
    {
        return new CasefileEnquiryLogEntry(Id, EntryText, EnteredBy, EntryDate);
    }
}
