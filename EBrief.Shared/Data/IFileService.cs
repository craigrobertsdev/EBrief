using EBrief.Shared.Models.Data;
using EBrief.Shared.Models.Shared;
using EBrief.Shared.Models.UI;

namespace EBrief.Shared.Data;
public interface IFileService
{
    Task SaveFile(CourtList courtList);
    Task<(CourtListEntry?, string?)> LoadCourtFile();
    (List<CourtListModel>? courtLists, string?) LoadLandscapeList(string filePath);
    Task<string?> SelectLandscapeList();
    void CreateDocumentDirectory();
    Task SaveDocument(Stream stream, string fileName);
    void DeleteDocuments(IEnumerable<string> documents);
}
