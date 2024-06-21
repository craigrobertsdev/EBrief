using EBrief.Models;
using EBrief.Models.Data;
using EBrief.Models.UI;

namespace EBrief.Data;

public interface IDataAccess
{
    public Task CreateCourtList(CourtListModel courtList);
    public Task<List<CourtListEntry>> GetSavedCourtLists();
    public Task UpdateCourtList(CourtList courtList);
    public Task AddCaseFiles(List<CaseFileModel> newCaseFiles, CourtList courtList);
    public Task DeleteCourtList(CourtListEntry courtList);
    public Task<CourtListModel?> GetCourtList(CourtCode courtCode, DateTime courtDate, int courtRoom);
    public Task<bool> CheckCourtListExists(CourtListEntry entry);
}