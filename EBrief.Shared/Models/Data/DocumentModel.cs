using EBrief.Shared.Models.Shared;
using EBrief.Shared.Models.UI;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EBrief.Shared.Models.Data;
public class DocumentModel : IDocument
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
    Casefile,
    Evidence
}
