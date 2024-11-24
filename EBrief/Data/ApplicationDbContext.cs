using EBrief.Shared.Models.Data;
using EBrief.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.IO;
using EBrief.Shared.Models.UI;

namespace EBrief.Data;
public class ApplicationDbContext : DbContext
{
    public DbSet<CourtListModel> CourtLists { get; set; } = default!;
    public DbSet<CasefileModel> Casefiles { get; set; } = default!;
    public DbSet<DefendantModel> Defendants { get; set; } = default!;
    public DbSet<DocumentModel> Documents { get; set; } = default!;
    public DbSet<CourtSittingModel> CourtSittings { get; set; } = default!;

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
        modelBuilder.Entity<CourtListModel>()
            .HasMany(cl => cl.Casefiles)
            .WithOne(cf => cf.CourtList)
            .HasForeignKey(cf => cf.CourtListId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CasefileModel>()
            .HasOne(cf => cf.CourtList)
            .WithMany(cl => cl.Casefiles)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CasefileModel>()
            .HasOne(cf => cf.Defendant)
            .WithMany()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CasefileModel>()
            .HasOne(cf => cf.Counsel)
            .WithMany()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CasefileModel>()
            .HasMany(cf => cf.Documents)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CasefileModel>()
            .HasMany(cf => cf.Schedule)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CasefileModel>()
            .HasMany(cf => cf.Charges)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CasefileModel>()
            .Ignore(cf => cf.Information);

        modelBuilder.Entity<CasefileModel>()
            .HasMany(cf => cf.CfelEntries)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CourtSittingModel>()
            .HasMany(cs => cs.Defendants)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

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
