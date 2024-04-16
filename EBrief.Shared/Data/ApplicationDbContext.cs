using EBrief.Shared.Helpers;
using EBrief.Shared.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace EBrief.Shared.Data;
public class ApplicationDbContext : DbContext
{
    public DbSet<CourtListModel> CourtLists { get; set; }
    public DbSet<CaseFileModel> CaseFiles { get; set; }
    public ApplicationDbContext()
    {
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        string dbPath = Path.Combine(FileHelpers.AppDataPath, "EBrief.db");
        options.UseSqlite($"Filename={dbPath}");
    }
}
