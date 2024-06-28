using EBrief.Models;
using EBrief.Models.UI;
using System.IO;

namespace EBrief.Data;
public interface IFileService
{
    Task SaveFile(CourtList courtList);
    Task<(CourtListEntry?, string?)> LoadCourtFile();
    void CreateCorrespondenceDirectory();
    void CreateEvidenceDirectory();
    Task SaveDocument(Stream stream, string fileName, FolderType folderType);
}
