using EBrief.Models;
using EBrief.Models.UI;

namespace EBrief.Data;
public interface IFileService
{
    Task SaveFile(CourtList courtList);
    Task<(CourtListEntry?, string)> LoadCourtFile();
}
