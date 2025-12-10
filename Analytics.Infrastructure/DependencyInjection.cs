using Analytics.Application.Abstractions;
using Analytics.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Analytics.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["ANALYTICS_DB_CONNECTION"]
                               ?? configuration.GetConnectionString("AnalyticsDb")
                               ?? throw new InvalidOperationException("Connection string not configured (ANALYTICS_DB_CONNECTION).");

        services.AddDbContext<AnalyticsDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IUserEventsRepository, UserEventsRepository>();

        return services;
    }
}

