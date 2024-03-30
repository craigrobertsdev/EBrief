﻿using CourtSystem.Models.Data;
using CourtSystem.Models.UI;
using Microsoft.EntityFrameworkCore;

namespace CourtSystem.Data;
public class CourtListDataAccess {
    private readonly ApplicationDbContext _context;

    public CourtListDataAccess() {
        _context = new ApplicationDbContext();
    }

    public void SaveCourtList(CourtListModel courtList) {
        _context.CourtLists.Add(courtList);
        _context.SaveChanges();
    }

    public void UpdateCourtList(CourtList courtList) {
        var courtListModel = _context.CourtLists.First();
        foreach (var caseFile in courtList.GetCaseFiles()) {
            courtListModel.CaseFiles.First(cf => cf.CaseFileNumber == caseFile.CaseFileNumber).Notes = caseFile.Notes;
        }

        _context.CourtLists.Update(courtListModel);
        _context.SaveChanges();
    }

    public void ClearDatabase() {
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
    }

    public CourtListModel? GetCourtList() {
        return _context.CourtLists
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
            .FirstOrDefault();
    }
}
