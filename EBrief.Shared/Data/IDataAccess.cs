using EBrief.Shared.Models.Data;
using EBrief.Shared.Models.Shared;
using EBrief.Shared.Models.UI;

namespace EBrief.Shared.Data;

public interface IDataAccess
{
    Task CreateCourtList(CourtListModel courtList);
    Task<List<CourtListEntry>> GetSavedCourtLists();
    Task UpdateCourtList(CourtList courtList);
    Task UpdateCfels(IEnumerable<string> casefileNumbers, string updateText);
    Task AddCasefiles(List<CasefileModel> newCasefiles, CourtList courtList);
    Task UpdateCasefiles(List<CasefileModel> newCasefiles);
    Task DeleteCourtList(CourtListEntry courtList);
    Task<CourtListModel?> GetCourtList(CourtCode courtCode, DateTime courtDate, int courtRoom);
    Task<bool> CheckCourtListExists(CourtListEntry entry);
    Task UpdateDocumentName(string fileName, string newFileName);
}