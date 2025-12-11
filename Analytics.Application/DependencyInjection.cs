using Analytics.Application.Recommendations;
using Analytics.Application.UserEvents;
using Microsoft.Extensions.DependencyInjection;

namespace Analytics.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AddUserEventHandler>();
        services.AddScoped<GetPopularCollectionsHandler>();
        services.AddScoped<GetRecentCollectionsHandler>();
        services.AddScoped<GetRecentTracksHandler>();
        return services;
    }
}

