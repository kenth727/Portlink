using ApprenticeApp.Core.Data.Configurations;
using ApprenticeApp.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApprenticeApp.Core.Data;

public class ApprenticeDbContext : DbContext
{
    public ApprenticeDbContext(DbContextOptions<ApprenticeDbContext> options) : base(options)
    {
    }

    public DbSet<Apprentice> Apprentices => Set<Apprentice>();
    public DbSet<Mentor> Mentors => Set<Mentor>();
    public DbSet<Assignment> Assignments => Set<Assignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ApprenticeConfiguration());
        modelBuilder.ApplyConfiguration(new MentorConfiguration());
        modelBuilder.ApplyConfiguration(new AssignmentConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
