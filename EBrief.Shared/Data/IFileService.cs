using EBrief.Shared.Models;
using EBrief.Shared.Models.Data;
using EBrief.Shared.Models.UI;

namespace EBrief.Shared.Data;
public interface IFileService
{
    Task SaveFile(CourtList courtList);
    Task<(CourtListEntry?, string?)> LoadCourtFile();
    (List<CourtListModel>? courtLists, string?) LoadLandscapeList(string filePath);
    string? SelectLandscapeList();
    //(CourtListModel? courtList, string?) LoadIndividualCourtList(string filePath);
    void CreateDocumentDirectory();
    Task SaveDocument(Stream stream, string fileName);
}
