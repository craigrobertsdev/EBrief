using EBrief.Models.UI;
using System.ComponentModel.DataAnnotations;

namespace EBrief.Models.Data;

public class OccurrenceDocumentModel
{
    [Key]
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;

    public OccurrenceDocument ToUIModel()
    {
        return new OccurrenceDocument
        {
            Id = Id,
            Title = Title,
            FileName = FileName
        };
    }
}