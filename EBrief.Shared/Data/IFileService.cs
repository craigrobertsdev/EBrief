using EBrief.Shared.Models;
using EBrief.Shared.Models.UI;

namespace EBrief.Shared.Data;
public interface IFileService
{
    Task SaveFile(CourtList courtList);
    Task<(CourtListEntry?, string?)> LoadCourtFile();
    void CreateDocumentDirectory();
    Task SaveDocument(Stream stream, string fileName);
}
