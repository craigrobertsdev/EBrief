using EBrief.Shared.Models.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EBrief.Data.Configuration;
public class DefendantConfiguration : IEntityTypeConfiguration<DefendantModel>
{
    public void Configure(EntityTypeBuilder<DefendantModel> builder)
    {
        builder.OwnsMany(d => d.InterventionOrders, iob =>
        {
            iob.ToTable("InterventionOrders");
            iob.WithOwner();
            iob.OwnsMany(io => io.Conditions);
        });

        builder.OwnsMany(d => d.BailAgreements, bab =>
        {
            bab.ToTable("BailAgreements");
            bab.WithOwner();
            bab.OwnsMany(ba => ba.Conditions);
        });
    }
}
