using EBrief.Shared.Models.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EBrief.Data.Configuration;
public class CasefileModelConfiguration : IEntityTypeConfiguration<CasefileModel>
{
    public void Configure(EntityTypeBuilder<CasefileModel> builder)
    {
        builder.HasOne(cf => cf.CourtList)
            .WithMany(cl => cl.Casefiles)
            .HasForeignKey(cf => cf.CourtListId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cf => cf.Defendant)
            .WithMany()
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsOne(cf => cf.Counsel)
            .WithOwner();

        builder.OwnsMany(cf => cf.Documents)
            .WithOwner();

        builder.OwnsMany(cf => cf.Schedule)
            .WithOwner();

        builder.OwnsMany(cf => cf.Charges)
            .WithOwner();

        builder.Ignore(cf => cf.Information);

        builder.OwnsMany(cf => cf.CfelEntries)
            .WithOwner();
    }
}
