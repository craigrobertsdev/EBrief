using EBrief.Shared.Models;
using EBrief.Shared.Models.Data;
using EBrief.Shared.Models.UI;

namespace EBrief.Shared.Data;

public interface IDataAccess
{
    public Task SaveCourtList(CourtListModel courtList);
    public Task<List<CourtListEntry>> GetSavedCourtLists();
    public Task UpdateCourtList(CourtList courtList);
    public Task AddCaseFiles(List<CaseFileModel> newCaseFiles, CourtList courtList);
    public Task DeleteCourtList(CourtListEntry courtList);
    public Task<CourtListModel?> GetCourtList(CourtCode courtCode, DateTime courtDate, int courtRoom);
    public Task<bool> CheckCourtListExists(CourtListEntry entry);
}