using EBrief.Models.Data;
using EBrief.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.IO;

namespace EBrief.Data;
public class ApplicationDbContext : DbContext
{
    public DbSet<CourtListModel> CourtLists { get; set; }
    public DbSet<CaseFileModel> CaseFiles { get; set; }
    public DbSet<DefendantModel> Defendants { get; set; }
    public DbSet<DocumentModel> Documents { get; set; }
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
            .HasMany(cl => cl.CaseFiles)
            .WithOne(cf => cf.CourtList)
            .HasForeignKey(cf => cf.CourtListId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CaseFileModel>()
            .HasOne(cf => cf.CourtList)
            .WithMany(cl => cl.CaseFiles)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CaseFileModel>()
            .HasOne(cf => cf.Defendant)
            .WithMany()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CaseFileModel>()
            .HasMany(cf => cf.Documents)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CaseFileModel>()
            .HasMany(cf => cf.Schedule)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CaseFileModel>()
            .HasMany(cf => cf.Charges)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CaseFileModel>()
            .Ignore(cf => cf.Information);

        modelBuilder.Entity<CaseFileModel>()
            .HasMany(cf => cf.CfelEntries)
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
