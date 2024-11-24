using EBrief.Shared.Data;
using EBrief.Shared.Models.Data;
using EBrief.Shared.Models.Shared;
using EBrief.Shared.Models.UI;
using EBrief.Shared.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EBrief.Data;
public class DataAccess : IDataAccess
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DataAccess> _logger;

    public DataAccess(ApplicationDbContext context, ILogger<DataAccess> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task CreateCourtList(CourtListModel courtList)
    {
        var courtListEntry = new CourtListEntry(courtList.CourtCode, courtList.CourtDate, courtList.CourtRoom);
        if (await CheckCourtListExists(courtListEntry))
        {
            return;
        }

        courtList.CombineAndSortDefendantCasefiles();

        _context.CourtLists.Add(courtList);
        _context.SaveChanges();
    }

    public async Task<List<CourtListEntry>> GetSavedCourtLists()
    {
        var courtListEntries = await _context.CourtLists
            .Select(e => new CourtListEntry(e.CourtCode, e.CourtDate, e.CourtRoom))
            .ToListAsync();

        // sort in descending order
        courtListEntries.Sort((c1, c2) => c2.CourtDate.CompareTo(c1.CourtDate));

        return courtListEntries;
    }

    public async Task UpdateCourtList(CourtList courtList)
    {
        var courtListModel = await _context.CourtLists
            .Include(cl => cl.Casefiles)
            .FirstAsync(cl => cl.Id == courtList.Id);
        foreach (var casefile in courtList.GetCasefiles())
        {
            courtListModel.Casefiles.First(cf => cf.CasefileNumber == casefile.CasefileNumber).Notes = casefile.Notes.Text;
        }

        _context.CourtLists.Update(courtListModel);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateCfels(IEnumerable<string> casefileNumbers, CasefileEnquiryLogEntry entry)
    {
        var casefilesToUpdate = _context.Casefiles.Where(cf => casefileNumbers.Contains(cf.CasefileNumber))
            .ToDictionary(cf => cf.CasefileNumber);

        foreach (var casefile in casefileNumbers)
        {
            casefilesToUpdate[casefile].CfelEntries
                .Add(entry.ToDbModel());
        }

        await _context.SaveChangesAsync();
    }

    public async Task AddCasefiles(List<CasefileModel> newCasefiles, CourtList courtList)
    {
        var existingCourtList = await _context.CourtLists
            .Where(cl => cl.CourtDate == courtList.CourtDate && cl.CourtCode == courtList.CourtCode && cl.CourtRoom == courtList.CourtRoom)
            .FirstOrDefaultAsync();

        if (existingCourtList is null)
        {
            _logger.LogError("AddCasefiles - CourtListNotFound - {date:ddMMyyyy} / {courtCode} / {courtRoom}", courtList.CourtDate, courtList.CourtCode, courtList.CourtRoom);
            throw new Exception("Error adding new case files.");
        }

        foreach (var cf in newCasefiles)
        {
            cf.CasefileNumber = cf.CasefileNumber.ToUpper();
        }
        existingCourtList.Casefiles.AddRange(newCasefiles);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCourtList(CourtListEntry entry)
    {
        var courtList = await _context.CourtLists.FirstOrDefaultAsync(e => e.CourtCode == entry.CourtCode && e.CourtDate == entry.CourtDate && e.CourtRoom == entry.CourtRoom);
        if (courtList is not null)
        {
            try
            {
                _context.CourtLists.Remove(courtList);
                _context.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
    }

    public async Task<CourtListModel?> GetCourtList(CourtCode courtCode, DateTime courtDate, int courtRoom)
    {
        return await _context.CourtLists
            .Where(cl => cl.CourtCode == courtCode && cl.CourtDate == courtDate && cl.CourtRoom == courtRoom)
            .Include(cl => cl.Casefiles)
            .ThenInclude(cf => cf.Documents)
            .Include(cl => cl.Casefiles)
            .ThenInclude(cf => cf.CfelEntries)
            .Include(cl => cl.Casefiles)
            .ThenInclude(cf => cf.Charges)
            .Include(cl => cl.Casefiles)
            .ThenInclude(cf => cf.Defendant)
            .ThenInclude(d => d.BailAgreements)
            .ThenInclude(ba => ba.Conditions)
            .Include(cl => cl.Casefiles)
            .ThenInclude(cf => cf.Defendant)
            .ThenInclude(d => d.InterventionOrders)
            .ThenInclude(io => io.ProtectedPerson)
            .Include(cl => cl.Casefiles)
            .ThenInclude(cf => cf.Defendant)
            .ThenInclude(d => d.InterventionOrders)
            .ThenInclude(io => io.Conditions)
            .Include(cl => cl.Casefiles)
            .ThenInclude(cf => cf.Schedule)
            .AsSplitQuery()
            .FirstOrDefaultAsync();
    }

    public async Task<bool> CheckCourtListExists(CourtListEntry entry)
    {
        var courtListEntry = await _context.CourtLists
            .Where(cl => cl.CourtDate == entry.CourtDate && cl.CourtCode == entry.CourtCode && cl.CourtRoom == entry.CourtRoom)
            .FirstOrDefaultAsync();

        return courtListEntry != null;
    }

    public async Task UpdateDocumentName(string fileName, string newFileName)
    {
        var document = _context.Documents.FirstOrDefault(d => d.FileName == fileName);
        if (document is not null)
        {
            document.FileName = newFileName;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateCasefiles(List<CasefileModel> newCasefiles)
    {
        var casefileNumbers = newCasefiles.Select(cf => cf.CasefileNumber);
        var existingCasefiles = await _context.Casefiles
            .Where(cf => casefileNumbers.Contains(cf.CasefileNumber))
            .ToListAsync();

        existingCasefiles.Sort((a, b) => a.CasefileNumber.CompareTo(b.CasefileNumber));
        newCasefiles.Sort((a, b) => a.CasefileNumber.CompareTo(b.CasefileNumber));

        for (int i = 0; i < existingCasefiles.Count; i++)
        {
            existingCasefiles[i].Update(newCasefiles[i]);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<List<CourtSitting>> GetCourtSittings(CourtListEntry entry)
    {
        var courtSittings = await _context.CourtSittings
            .Where(cs => cs.CourtCode == entry.CourtCode && cs.CourtRoom == entry.CourtRoom && cs.SittingTime.Date == entry.CourtDate.Date)
            .ToListAsync();

        return courtSittings.ToUIModels();
    }

    public async Task UpdateCourtSittings(List<CourtSitting> courtSittings, CourtListEntry entry)
    {
        var existingCourtSittings = await _context.CourtSittings
            .Where(cs => cs.CourtCode == entry.CourtCode && cs.CourtRoom == entry.CourtRoom && cs.SittingTime.Date == entry.CourtDate.Date)
            .ToListAsync();

        foreach (var courtSitting in courtSittings)
        {
            var existing = existingCourtSittings.FirstOrDefault(cs => cs.SittingTime == courtSitting.SittingTime);
            if (existing is not null)
            {
                existing.Defendants = courtSitting.Defendants.ToDataModels();
            }
            else
            {
                _context.CourtSittings.Add(courtSitting.ToDataModel());
            }
        }

        await _context.SaveChangesAsync();
    }
}
