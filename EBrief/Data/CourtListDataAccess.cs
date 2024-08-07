using EBrief.Shared.Data;
using EBrief.Shared.Models.Data;
using EBrief.Shared.Models.Shared;
using EBrief.Shared.Models.UI;
using EBrief.Shared.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EBrief.Data;
public class CourtListDataAccess : IDataAccess
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CourtListDataAccess> _logger;
    private readonly AppState _appState;

    public CourtListDataAccess(ApplicationDbContext context, ILogger<CourtListDataAccess> logger, AppState appState)
    {
        _context = context;
        _logger = logger;
        _appState = appState;
    }

    public async Task CreateCourtList(CourtListModel courtList)
    {
        var courtListEntry = new CourtListEntry(courtList.CourtCode, courtList.CourtDate, courtList.CourtRoom);
        if (await CheckCourtListExists(courtListEntry))
        {
            return;
        }

        courtList.CombineAndSortDefendantCaseFiles();

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
        var courtListModel = await _context.CourtLists.FirstAsync(cl => cl.Id == courtList.Id);
        foreach (var caseFile in courtList.GetCaseFiles())
        {
            courtListModel.CaseFiles.First(cf => cf.CaseFileNumber == caseFile.CaseFileNumber).Notes = caseFile.Notes.Text;
        }

        _context.CourtLists.Update(courtListModel);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateCaseFiles(IEnumerable<string> caseFileNumbers, string updateText)
    {
        var caseFilesToUpdate = _context.CaseFiles.Where(cf => caseFileNumbers.Contains(cf.CaseFileNumber))
            .ToDictionary(cf => cf.CaseFileNumber);

        foreach (var caseFile in caseFileNumbers)
        {
            caseFilesToUpdate[caseFile].CfelEntries
                .Add(new()
                {
                    EntryText = updateText,
                    EnteredBy = _appState.CurrentUser,
                    EntryDate = DateTime.Now,
                });
        }

        await _context.SaveChangesAsync();
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
            .Include(cl => cl.CaseFiles)
            .ThenInclude(cf => cf.Documents)
            .Include(cl => cl.CaseFiles)
            .ThenInclude(cf => cf.CfelEntries)
            .Include(cl => cl.CaseFiles)
            .ThenInclude(cf => cf.Charges)
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
}
