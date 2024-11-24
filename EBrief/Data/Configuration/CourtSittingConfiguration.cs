using EBrief.Shared.Models.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EBrief.Data.Configuration;
public class CourtSittingConfiguration : IEntityTypeConfiguration<CourtSittingModel>
{
    public void Configure(EntityTypeBuilder<CourtSittingModel> builder)
    {
        builder.HasMany(cs => cs.Defendants)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
