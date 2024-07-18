using EBrief.Models.UI;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EBrief.Models.Data;

public class OccurrenceDocumentModel
{
    [Key]
    [JsonIgnore]
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