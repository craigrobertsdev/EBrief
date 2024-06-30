using EBrief.Models;
using EBrief.Models.Data;
using EBrief.Models.UI;

namespace EBrief.Data;

public interface IDataAccess
{
    Task CreateCourtList(CourtListModel courtList);
    Task<List<CourtListEntry>> GetSavedCourtLists();
    Task UpdateCourtList(CourtList courtList);
    Task UpdateCaseFiles(IEnumerable<string> caseFileNumbers, string updateText);
    Task AddCaseFiles(List<CaseFileModel> newCaseFiles, CourtList courtList);
    Task DeleteCourtList(CourtListEntry courtList);
    Task<CourtListModel?> GetCourtList(CourtCode courtCode, DateTime courtDate, int courtRoom);
    Task<bool> CheckCourtListExists(CourtListEntry entry);
}