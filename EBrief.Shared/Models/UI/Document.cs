using EBrief.Shared.Models.Shared;

namespace EBrief.Shared.Models.UI;
public class Document : IDocument
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}
