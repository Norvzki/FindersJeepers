namespace FindersJeepers.Infrastructure;
public static class DependencyInjeciton
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IDriverRepository, DriverRepository>();
        services.AddScoped<IJeepneyRepository, JeepneyRepository>();
        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddScoped<ITripRepository, TripRepository>();
        services.AddScoped<IRouteRepository, RouteRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}