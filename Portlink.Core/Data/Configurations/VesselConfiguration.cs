using PortlinkApp.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PortlinkApp.Core.Data.Configurations;

public class VesselConfiguration : IEntityTypeConfiguration<Vessel>
{
    public void Configure(EntityTypeBuilder<Vessel> builder)
    {
        builder.Property(v => v.Name).HasMaxLength(100).IsRequired();
        builder.Property(v => v.ImoNumber).HasMaxLength(20).IsRequired();
        builder.Property(v => v.FlagCountry).HasMaxLength(100).IsRequired();
        builder.Property(v => v.CargoType).HasMaxLength(100);
        builder.Property(v => v.OwnerCompany).HasMaxLength(200);
        builder.Property(v => v.AgentEmail).HasMaxLength(200);

        builder.Property(v => v.LengthOverall).HasPrecision(10, 2);
        builder.Property(v => v.Beam).HasPrecision(10, 2);
        builder.Property(v => v.Draft).HasPrecision(10, 2);

        builder.HasIndex(v => v.ImoNumber).IsUnique();

        builder.HasMany(v => v.PortCalls)
            .WithOne(pc => pc.Vessel)
            .HasForeignKey(pc => pc.VesselId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
