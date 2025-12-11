using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Analytics.Infrastructure.Persistence;

public class AnalyticsDbContextFactory : IDesignTimeDbContextFactory<AnalyticsDbContext>
{
    public AnalyticsDbContext CreateDbContext(string[] args)
    {
        // Prefer environment variable
        var connectionString = Environment.GetEnvironmentVariable("ANALYTICS_DB_CONNECTION");

        // Fallback to configuration sources (appsettings, user-secrets) under the API project
        if (string.IsNullOrEmpty(connectionString))
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "Analytics.API");
            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                // user-secrets tied to the API project (has UserSecretsId in csproj)
                .AddUserSecrets<AnalyticsDbContextFactory>(optional: true)
                .AddEnvironmentVariables()
                .Build();

            connectionString = configuration["ANALYTICS_DB_CONNECTION"]
                               ?? configuration.GetConnectionString("AnalyticsDb");
        }

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string not found. Set ANALYTICS_DB_CONNECTION environment variable or configure it in appsettings.json");
        }

        var optionsBuilder = new DbContextOptionsBuilder<AnalyticsDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new AnalyticsDbContext(optionsBuilder.Options);
    }
}

