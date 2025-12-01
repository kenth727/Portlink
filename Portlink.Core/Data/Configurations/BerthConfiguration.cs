using PortlinkApp.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PortlinkApp.Core.Data.Configurations;

public class BerthConfiguration : IEntityTypeConfiguration<Berth>
{
    public void Configure(EntityTypeBuilder<Berth> builder)
    {
        builder.Property(b => b.BerthCode).HasMaxLength(50).IsRequired();
        builder.Property(b => b.TerminalName).HasMaxLength(200).IsRequired();
        builder.Property(b => b.Facilities).HasMaxLength(500);
        builder.Property(b => b.Notes).HasMaxLength(1000);

        builder.Property(b => b.MaxVesselLength).HasPrecision(10, 2);
        builder.Property(b => b.MaxDraft).HasPrecision(10, 2);

        builder.HasIndex(b => b.BerthCode).IsUnique();

        builder.HasMany(b => b.PortCalls)
            .WithOne(pc => pc.Berth)
            .HasForeignKey(pc => pc.BerthId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
