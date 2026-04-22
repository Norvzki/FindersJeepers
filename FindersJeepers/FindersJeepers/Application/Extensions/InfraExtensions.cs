namespace FindersJeepers.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDriverService, DriverService>();
        //services.AddScoped<IJeepneyRepository, JeepneyRepository>();
        //services.AddScoped<ILocationRepository, LocationRepository>();
        //services.AddScoped<ITripRepository, TripRepository>();
        //services.AddScoped<IRouteRepository, RouteRepository>();

        return services;
    }
}