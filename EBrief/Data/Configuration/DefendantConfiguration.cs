using EBrief.Shared.Models.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EBrief.Data.Configuration;
public class DefendantConfiguration : IEntityTypeConfiguration<DefendantModel>
{
    public void Configure(EntityTypeBuilder<DefendantModel> builder)
    {
        builder.HasMany(d => d.BailAgreements)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.InterventionOrders)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class BailAgreementConfiguration : IEntityTypeConfiguration<BailAgreementModel>
{
    public void Configure(EntityTypeBuilder<BailAgreementModel> builder)
    {
        builder.HasMany(ba => ba.Conditions)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class InterventionOrderConfiguration : IEntityTypeConfiguration<InterventionOrderModel>
{
    public void Configure(EntityTypeBuilder<InterventionOrderModel> builder)
    {
        builder.HasMany(io => io.Conditions)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
