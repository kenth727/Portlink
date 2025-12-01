using PortlinkApp.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PortlinkApp.Core.Data.Configurations;

public class PortCallConfiguration : IEntityTypeConfiguration<PortCall>
{
    public void Configure(EntityTypeBuilder<PortCall> builder)
    {
        builder.Property(pc => pc.CargoDescription).HasMaxLength(100);
        builder.Property(pc => pc.CargoUnit).HasMaxLength(50);
        builder.Property(pc => pc.Notes).HasMaxLength(1000);
        builder.Property(pc => pc.DelayReason).HasMaxLength(200);

        builder.Property(pc => pc.CargoQuantity).HasPrecision(18, 2);

        builder.HasOne(pc => pc.Vessel)
            .WithMany(v => v.PortCalls)
            .HasForeignKey(pc => pc.VesselId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pc => pc.Berth)
            .WithMany(b => b.PortCalls)
            .HasForeignKey(pc => pc.BerthId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(pc => pc.EstimatedTimeOfArrival);
        builder.HasIndex(pc => new { pc.VesselId, pc.Status });
    }
}
