

using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

public class LocationService : ILocationService
{
    private readonly IUnitOfWork _uow;

    public LocationService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task CreateAsync(CreateLocationRequest request)
    {
        var locationExists = await _uow.Locations.Get()
            .Where(x => x.Name == request.Name)
            .AnyAsync();
        if (locationExists)
            throw new ApplicationException("A location already exists by this name!");

        await _uow.Locations.AddAsync(Location.Create(request.Name, request.Description));
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteAsync(int locationId)
    {
        var location = await _uow.Locations.GetByIdAsync(locationId);

        var routesWithLocation = await _uow.Routes.GetByLocationAsync(locationId);

        routesWithLocation = routesWithLocation.Where(x => !x.IsDeleted).ToList();

        if (routesWithLocation.Any())
            throw new ApplicationException("You cannot delete a location that is used by routes!");

        location.Delete();
        _uow.Locations.Update(location);
        await _uow.SaveChangesAsync();
    }

    public async Task<List<LocationDto>> GetAsync(int pageNumber = -1, int pageSize = -1)
    {
        var locations = _uow.Locations.Get();
        return await locations.Select(l => new LocationDto
        {
            Id = l.Id,
            Name = l.Name,
            Description = l.Description,
        }).ToListAsync();
    }

    public async Task<LocationDetail> GetByIdAsync(int locationId)
    {
        var location = await _uow.Locations.GetByIdAsync(locationId);
        if (location == null) return null;

        var routesQuery = _uow.Routes.Get();
        var locationsQuery = _uow.Locations.Get();

        var routes = await (from route in routesQuery
                            join l1 in locationsQuery on route.LocationStartId equals l1.Id
                            join l2 in locationsQuery on route.LocationEndId equals l2.Id
                            where route.LocationStartId == locationId || route.LocationEndId == locationId
                            select new RouteSummary
                            {
                                Id = route.Id,
                                RouteCode = route.RouteCode,
                                LocationStart = l1.Name,
                                LocationEnd = l2.Name,
                            }).ToListAsync();

        var stops = await (from route in routesQuery
                           where route.Stops.Any(x => x.LocationId == locationId)
                           select new RouteStopOccurrence
                           {
                               RouteCode = route.RouteCode,
                               RouteId = route.Id,
                               StopIndex = route.Stops
                                                .Where(x => x.LocationId == locationId)
                                                .Select(x => x.StopIndex)
                                                .FirstOrDefault()
                           }).ToListAsync();

        return new LocationDetail
        {
            Id = location.Id,
            Name = location.Name,
            Description = location.Description,
            Routes = routes,
            StopOccurrences = stops
        };
    }

    public async Task UpdateAsync(UpdateLocationRequest request)
    {
        var location = await _uow.Locations.GetByIdAsync(request.Id);

        var locationExists = await _uow.Locations.Get()
            .Where(x => x.Name == request.Name && x.Id != request.Id)
            .AnyAsync();

        if (locationExists) throw new ApplicationException("There's already a location with this name!");

        location.UpdateInformation(request.Name, request.Description);
        _uow.Locations.Update(location);
        await _uow.SaveChangesAsync();
    }
}

