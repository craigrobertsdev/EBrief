using EBrief.Shared.Models;
using EBrief.Shared.Models.Data;
using EBrief.Shared.Models.UI;
using Microsoft.EntityFrameworkCore;

namespace EBrief.Shared.Data;
public class CourtListDataAccess
{
    private readonly ApplicationDbContext _context;

    public CourtListDataAccess()
    {
        _context = new ApplicationDbContext();
    }

    public void SaveCourtList(CourtListModel courtList)
    {
        /* An error is occurring where the case files already exist in the database, but a court list with different details is trying to be added
         * This results in an empty court list as all the pre-existing casefiles are filtered out leaving nothing to be associated with the 
         * new court list.
         * 
         * A better solution would be to have a different key for case files so they can exist multiple times in the same database.
         */
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

    public CourtListModel? GetCourtList(CourtCode courtCode, DateTime courtDate, int courtRoom)
    {
        return _context.CourtLists
            .Where(cl => cl.CourtCode == courtCode)
            .Where(cl => cl.CourtDate == courtDate)
            .Where(cl => cl.CourtRoom == courtRoom)
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
