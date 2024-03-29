using CourtSystem.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace CourtSystem.Data;
public class ApplicationDbContext : DbContext {
    public DbSet<CourtListModel> CourtLists { get; set; }
    public DbSet<CaseFileModel> CaseFiles { get; set; }
    public ApplicationDbContext() {
        this.Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options) {
        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "CourtSystem.db");
        options.UseSqlite($"Filename={dbPath}");
    }
}
