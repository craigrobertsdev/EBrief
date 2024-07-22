using EBrief.Shared.Models.Data;
using System.ComponentModel.DataAnnotations;

namespace EBrief.Shared.Models.UI;
public class CaseFileEnquiryLogEntry
{
    [Key]
    public int Id { get; set; }
    public string EntryText { get; set; }
    public string EnteredBy { get; set; }

    public DateTime EntryDate { get; set; }
    public CaseFileEnquiryLogEntry(string entryText, string enteredBy, DateTime? entryDate = null)
    {
        EntryText = entryText;
        EnteredBy = enteredBy;
        EntryDate = entryDate ?? DateTime.Now;
    }
    public CaseFileEnquiryLogEntry(int id, string entryText, string enteredBy, DateTime entryDate)
    {
        Id = id;
        EntryText = entryText;
        EnteredBy = enteredBy;
        EntryDate = entryDate;
    }
}
