using ApprenticeApp.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApprenticeApp.Core.Data.Configurations;

public class MentorConfiguration : IEntityTypeConfiguration<Mentor>
{
    public void Configure(EntityTypeBuilder<Mentor> builder)
    {
        builder.Property(m => m.Name).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Email).HasMaxLength(200).IsRequired();

        builder.HasIndex(m => m.Email).IsUnique();

        builder.HasMany(m => m.Assignments)
            .WithOne(a => a.Mentor)
            .HasForeignKey(a => a.MentorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
