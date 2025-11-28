using ApprenticeApp.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ApprenticeApp.Api;

public class ApprenticeDbContextFactory : IDesignTimeDbContextFactory<ApprenticeDbContext>
{
    public ApprenticeDbContext CreateDbContext(string[] args)
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? "Data Source=apprentice.db";
        var envConnection = Environment.GetEnvironmentVariable("APPRENTICEAPP_CONNECTION");
        if (!string.IsNullOrWhiteSpace(envConnection))
        {
            connectionString = envConnection;
        }

        var optionsBuilder = new DbContextOptionsBuilder<ApprenticeDbContext>();
        optionsBuilder.UseSqlite(connectionString, builder => builder.MigrationsAssembly("ApprenticeApp.Api"));
        return new ApprenticeDbContext(optionsBuilder.Options);
    }
}
