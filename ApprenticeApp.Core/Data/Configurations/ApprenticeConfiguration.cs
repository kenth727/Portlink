using ApprenticeApp.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApprenticeApp.Core.Data.Configurations;

public class ApprenticeConfiguration : IEntityTypeConfiguration<Apprentice>
{
    public void Configure(EntityTypeBuilder<Apprentice> builder)
    {
        builder.Property(a => a.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(a => a.LastName).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Email).HasMaxLength(200).IsRequired();

        builder.HasIndex(a => a.Email).IsUnique();

        builder.HasMany(a => a.Assignments)
            .WithOne(a => a.Apprentice)
            .HasForeignKey(a => a.ApprenticeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
