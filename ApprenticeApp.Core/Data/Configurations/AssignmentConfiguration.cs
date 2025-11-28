using ApprenticeApp.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApprenticeApp.Core.Data.Configurations;

public class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> builder)
    {
        builder.Property(a => a.Title).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Notes).HasMaxLength(1000);

        builder.HasOne(a => a.Apprentice)
            .WithMany(apprentice => apprentice.Assignments)
            .HasForeignKey(a => a.ApprenticeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Mentor)
            .WithMany(m => m.Assignments)
            .HasForeignKey(a => a.MentorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
