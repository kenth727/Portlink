using PortlinkApp.Core.Data.Configurations;
using PortlinkApp.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace PortlinkApp.Core.Data;

public class ApprenticeDbContext : IdentityDbContext<ApplicationUser>
{
    public ApprenticeDbContext(DbContextOptions<ApprenticeDbContext> options) : base(options)
    {
    }

    public DbSet<Vessel> Vessels => Set<Vessel>();
    public DbSet<Berth> Berths => Set<Berth>();
    public DbSet<PortCall> PortCalls => Set<PortCall>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Call base first for Identity tables

        modelBuilder.ApplyConfiguration(new VesselConfiguration());
        modelBuilder.ApplyConfiguration(new BerthConfiguration());
        modelBuilder.ApplyConfiguration(new PortCallConfiguration());
    }
}
