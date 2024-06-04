using EBrief.WebClient.Models.UI;

namespace EBrief.WebClient.Models.Data;

public class OccurrenceDocumentModel
{
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