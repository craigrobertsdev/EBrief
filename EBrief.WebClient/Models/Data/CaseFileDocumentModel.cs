using EBrief.WebClient.Models.UI;

namespace EBrief.WebClient.Models.Data;

public class CaseFileDocumentModel
{
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