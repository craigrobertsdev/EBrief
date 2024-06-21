using EBrief.Models.UI;
using System.ComponentModel.DataAnnotations;

namespace EBrief.Models.Data;

public class CaseFileDocumentModel
{
    [Key]
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;

    public CaseFileDocument ToUIModel()
    {
        return new CaseFileDocument
        {
            Id = Id,
            Title = Title,
            FileName = FileName
        };
    }
}