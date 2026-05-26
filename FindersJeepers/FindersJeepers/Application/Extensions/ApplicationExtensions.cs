namespace FindersJeepers.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDriverService, DriverService>();
        services.AddScoped<IJeepService, JeepService>();
        services.AddScoped<ILocationService, LocationService>();
        services.AddScoped<ITripService, TripService>();
        services.AddScoped<IRouteService, RouteService>();
        services.AddScoped<IOptionService, OptionService>();
        services.AddScoped<ISimulationService, SimulationService>();
        services.AddScoped<IGenerationService, GenerationService>();
        services.AddScoped<IDashboardService, DashboardService>(); // Crazy stuff -norbel

        return services;
    }
}