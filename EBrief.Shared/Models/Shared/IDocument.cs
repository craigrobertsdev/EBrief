using EBrief.Shared.Models.Data;

namespace EBrief.Shared.Models.Shared;
public interface IDocument
{
    int Id { get; set; }
    string Title { get; set; }
    string FileName { get; set; }
}
