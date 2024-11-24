using EBrief.Shared.Models.Data;
using EBrief.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.IO;

namespace EBrief.Data;
public class ApplicationDbContext : DbContext
{
    public DbSet<CasefileModel> Casefiles { get; set; } = default!;
    public DbSet<CourtListModel> CourtLists { get; set; } = default!;
    public DbSet<CourtSittingModel> CourtSittings { get; set; } = default!;
    public DbSet<DefendantModel> Defendants { get; set; } = default!;
    public DbSet<DocumentModel> Documents { get; set; } = default!;

    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {

            string dbPath = Path.Combine(FileHelpers.AppDataPath, "EBrief.db");
            optionsBuilder.UseSqlite($"Filename={dbPath}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

public class DbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DbContext>();

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
