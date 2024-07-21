using EBrief.Models.UI;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EBrief.Models.Data;
public class DocumentModel
{
    [Key]
    [JsonIgnore]
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }

    public Document ToUIModel()
    {
        return new Document
        {
            Id = Id,
            Title = Title,
            FileName = FileName
        };
    }
}

public enum DocumentType
{
    CaseFile,
    Occurrence
}
