using EBrief.Shared.Models;
using EBrief.Shared.Models.Data;
using EBrief.Shared.Models.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EBrief.Shared.Data;
public class CourtListDataAccess
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CourtListDataAccess> _logger;

    public CourtListDataAccess(ApplicationDbContext context, ILogger<CourtListDataAccess> logger)
    {
        _context = context;
        _logger = logger;
    }

    public void SaveCourtList(CourtListModel courtList)
    {
        var existingCourtList = _context.CourtLists
            .Where(cl => cl.CourtDate == courtList.CourtDate && cl.CourtCode == courtList.CourtCode && cl.CourtRoom == courtList.CourtRoom)
            .FirstOrDefault();

        if (existingCourtList != null)
        {
            return;
        }

        _context.CourtLists.Add(courtList);
        _context.SaveChanges();
    }

    public void UpdateCourtList(CourtList courtList)
    {
        var courtListModel = _context.CourtLists.First();
        foreach (var caseFile in courtList.GetCaseFiles())
        {
            courtListModel.CaseFiles.First(cf => cf.CaseFileNumber == caseFile.CaseFileNumber).Notes = caseFile.Notes;
        }

        _context.CourtLists.Update(courtListModel);
        _context.SaveChanges();
    }

    public async Task AddCaseFiles(List<CaseFileModel> newCaseFiles, CourtList courtList)
    {
        var existingCourtList = await _context.CourtLists
            .Where(cl => cl.CourtDate == courtList.CourtDate && cl.CourtCode == courtList.CourtCode && cl.CourtRoom == courtList.CourtRoom)
            .FirstOrDefaultAsync();

        if (existingCourtList is null)
        {
            _logger.LogError("AddCaseFiles - CourtListNotFound - {date:ddMMyyyy} / {courtCode} / {courtRoom}", courtList.CourtDate, courtList.CourtCode, courtList.CourtRoom);
            throw new Exception("Error adding new case files.");
        }

        existingCourtList.CaseFiles.AddRange(newCaseFiles);
        await _context.SaveChangesAsync();
    }

    public void DeleteCourtList(CourtCode courtCode, DateTime courtDate, int courtRoom)
    {
        var courtList = _context.CourtLists.FirstOrDefault(e => e.CourtCode == courtCode && e.CourtDate == courtDate && e.CourtRoom == courtRoom);
        if (courtList is not null)
        {
            try
            {
                _context.CourtLists.Remove(courtList);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                throw;
            }

        }
    }

    public CourtListModel? GetCourtList(CourtCode courtCode, DateTime courtDate, int courtRoom)
    {
        return _context.CourtLists
            .Where(cl => cl.CourtCode == courtCode && cl.CourtDate == courtDate && cl.CourtRoom == courtRoom)
            .Include(cl => cl.CaseFiles)
            .ThenInclude(cf => cf.CaseFileDocuments)
            .Include(cl => cl.CaseFiles)
            .ThenInclude(cf => cf.CfelEntries)
            .Include(cl => cl.CaseFiles)
            .ThenInclude(cf => cf.Charges)
            .Include(cl => cl.CaseFiles)
            .ThenInclude(cf => cf.OccurrenceDocuments)
            .Include(cl => cl.CaseFiles)
            .ThenInclude(cf => cf.Defendant)
            .ThenInclude(d => d.BailAgreements)
            .ThenInclude(ba => ba.Conditions)
            .Include(cl => cl.CaseFiles)
            .ThenInclude(cf => cf.Defendant)
            .ThenInclude(d => d.InterventionOrders)
            .ThenInclude(io => io.ProtectedPerson)
            .Include(cl => cl.CaseFiles)
            .ThenInclude(cf => cf.Defendant)
            .ThenInclude(d => d.InterventionOrders)
            .ThenInclude(io => io.Conditions)
            .Include(cl => cl.CaseFiles)
            .ThenInclude(cf => cf.PreviousHearings)
            .AsSplitQuery()
            .FirstOrDefault();
    }
}
