using EBrief.Shared.Helpers;
using EBrief.Shared.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace EBrief.Shared.Data;
public class ApplicationDbContext : DbContext
{
    public DbSet<CourtListModel> CourtLists { get; set; }
    public DbSet<CaseFileModel> CaseFiles { get; set; }
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
        Database.EnsureCreated();
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
            .HasMany(cf => cf.OccurrenceDocuments)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CaseFileModel>() 
            .HasMany(cf => cf.CaseFileDocuments)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CaseFileModel>() 
            .HasMany(cf => cf.PreviousHearings)
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
