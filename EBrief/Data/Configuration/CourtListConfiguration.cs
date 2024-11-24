using EBrief.Shared.Models.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EBrief.Data.Configuration;
public class CourtListConfiguration : IEntityTypeConfiguration<CourtListModel>
{
    public void Configure(EntityTypeBuilder<CourtListModel> builder)
    {
        builder.HasMany(cl => cl.Casefiles)
            .WithOne(cf => cf.CourtList)
            .HasForeignKey(cf => cf.CourtListId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
