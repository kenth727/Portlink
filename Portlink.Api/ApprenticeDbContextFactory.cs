using PortlinkApp.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PortlinkApp.Api;

public class ApprenticeDbContextFactory : IDesignTimeDbContextFactory<PortlinkDbContext>
{
    public PortlinkDbContext CreateDbContext(string[] args)
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=portlink;Username=portadmin;Password=Port@Dev2024";
        var envConnection = Environment.GetEnvironmentVariable("PORTLINK_CONNECTION")
            ?? Environment.GetEnvironmentVariable("APPRENTICEAPP_CONNECTION");
        if (!string.IsNullOrWhiteSpace(envConnection))
        {
            connectionString = envConnection;
        }

        var optionsBuilder = new DbContextOptionsBuilder<PortlinkDbContext>();
        optionsBuilder.UseNpgsql(
            connectionString,
            builder => builder.MigrationsAssembly(typeof(PortlinkDbContext).Assembly.GetName().Name));
        return new PortlinkDbContext(optionsBuilder.Options);
    }
}
