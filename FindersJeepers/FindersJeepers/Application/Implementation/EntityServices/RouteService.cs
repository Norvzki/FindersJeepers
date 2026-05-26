

using Microsoft.AspNetCore.Components.Routing;
using Microsoft.EntityFrameworkCore;

public class RouteService : IRouteService
{
    private readonly IUnitOfWork _uow;

    public RouteService(IUnitOfWork uow)
    {
        _uow = uow;
    }
    public async Task<List<RouteDto>> GetRoutesAsync()
    {
        var result = await (
        from r in _uow.Routes.Get()

        join ls in _uow.Locations.Get()
            on r.LocationStartId equals ls.Id

        join le in _uow.Locations.Get()
            on r.LocationEndId equals le.Id

        select new RouteDto
        {
            Id = r.Id,
            RouteCode = r.RouteCode,
            LocationStart = ls.Name,
            LocationEnd = le.Name,
            Stops = (from rs in r.Stops
                     join l in _uow.Locations.Get(null) on rs.LocationId equals l.Id
                    select new RouteStopDto
                    {
                        LocationId = rs.LocationId,
                        LocationName = l.Name,
                        StopIndex = rs.StopIndex
                    }).ToList()
        }
    ).ToListAsync();

        return result;
    }
    public async Task<RouteDetail> GetDetailAsync(int routeId)
    {
        var route = await _uow.Routes.GetByIdAsync(routeId);

        if (route == null || route.IsDeleted) throw new InvalidIdException("Invalid id or that route is deleted");

        var locationStart = await _uow.Locations.GetByIdAsync(route.LocationStartId);
        var locationEnd = await _uow.Locations.GetByIdAsync(route.LocationEndId);

        var stops = (from r in route.Stops
                     where r.Direction == RouteDirection.Forward
                     join l in _uow.Locations.Get() on r.LocationId equals l.Id
                     select new RouteStopDto
                     {
                         LocationId = l.Id,
                         LocationName = l.Name,
                         StopIndex = r.StopIndex,
                     }).ToList();
        var rStops = (from r in route.Stops
                      where r.Direction == RouteDirection.Return
                      join l in _uow.Locations.Get() on r.LocationId equals l.Id
                      select new RouteStopDto
                      {
                          LocationId = l.Id,
                          LocationName = l.Name,
                          StopIndex = r.StopIndex,
                      }).ToList();

        var assignedJeepneys = await (from j in _uow.Jeepneys.Get()
                                where j.RouteId == route.Id
                                select new JeepneySummary
                                {
                                    Id = j.Id,
                                    BodyNumber = j.BodyNumber,
                                    Capacity = j.Capacity,
                                    PlateNumber = j.PlateNumber,
                                    RouteCode = route.RouteCode,
                                }).ToListAsync();

        return new RouteDetail
        {
            AssignedJeepneys = assignedJeepneys,
            Id = route.Id,
            LocationEnd = new LocationDto
            {
                Id = locationEnd.Id,
                Name = locationEnd.Name,
                Description = locationEnd.Description,
            },
            LocationStart = new LocationDto
            {
                Id = locationStart.Id,
                Name = locationStart.Name,
                Description = locationStart.Description,
            },
            RouteCode = route.RouteCode,
            Stops = stops,
            ReturnStops = rStops
        };
    }
    public async Task CreateRouteAsync(CreateRouteRequest req)
    {
        var routeOfCode = await _uow.Routes.GetByRouteCodeAsync(req.RouteCode);
        if(routeOfCode != null)
            throw new ApplicationException("That route code is already taken!");

        var route = Route.Create(req.RouteCode, req.StartLocation, req.EndLocation);
        await _uow.Routes.AddAsync(route);
        await _uow.SaveChangesAsync();
    }
    public async Task AddRouteStopsAsync(AddRouteStopRequest req)
    {
        var currentTrip = await _uow.Trips.GetActiveTripsOnRouteAsync(req.RouteId);
        if (currentTrip.Any())
            throw new ApplicationException("You cannot change the stop sequence of a route if it's being used by an active trip!");

        var route = await _uow.Routes.GetByIdAsync(req.RouteId);

        if (req.RouteDirection == RouteDirection.Forward)
                route.ClearStops();
        else if (req.RouteDirection == RouteDirection.Return)
                route.ClearReturnStops();

            foreach (var stop in req.RouteStops)
            {
            if (stop.LocationId == route.LocationStartId || stop.LocationId == route.LocationEndId)
                throw new ApplicationException("You cannot assign a routestop that is already the route's start or end location!");

                route.AddStop(stop.LocationId, stop.Index, req.RouteDirection);
            }


        _uow.Routes.Update(route);
        await _uow.SaveChangesAsync();
    }


    public async Task DeleteAsync(int routeId)
    {
        var route = await _uow.Routes.GetByIdAsync(routeId);
        if (route == null || route.IsDeleted) throw new InvalidIdException("Invalid route ID!");

        var tripsUsingRoute = await _uow.Trips.GetActiveTripsOnRouteAsync(routeId);

        if (tripsUsingRoute.Any())
            throw new ApplicationException("You cannot delete a route that's currently being used in a trip!");

        var jeepsUsingRoute = await _uow.Jeepneys.GetByRouteAsync(routeId);
        await _uow.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);

        try
        {
            foreach(var jeep in jeepsUsingRoute)
            {
                jeep.ClearRoute();
                _uow.Jeepneys.Update(jeep);
            }
            route.Delete();
            _uow.Routes.Update(route);
            await _uow.SaveChangesAsync();
            await _uow.CommitAsync();
        } catch
        {
            await _uow.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateAsync(UpdateRouteRequest req)
    {
        var route = await _uow.Routes.GetByIdAsync(req.Id);
        var routeWithCode = await _uow.Routes.Get()
            .Where(x => x.RouteCode == req.RouteCode && x.Id != route.Id)
            .FirstOrDefaultAsync();

        if (routeWithCode != null)
            throw new ApplicationException("That route code is already taken!");

        var currentTrip = await _uow.Trips.GetActiveTripsOnRouteAsync(route.Id);
        if (currentTrip.Any())
            throw new ApplicationException("You cannot change the stop sequence of a route if it's being used by an active trip!");

        route.UpdateInformation(req.RouteCode, req.LocationStartId, req.LocationEndId);
        _uow.Routes.Update(route);
        await _uow.SaveChangesAsync();
    }

    public async Task AutoGenerateReturnRouteAsync(int routeId)
    {
        var activeTrips = await _uow.Trips.GetActiveTripsOnRouteAsync(routeId);
        if (activeTrips.Any())
            throw new ApplicationException("You cannot alter the route sequence while it has active trips running!");

        var route = await _uow.Routes.GetByIdAsync(routeId);
        if (route == null || route.IsDeleted)
            throw new InvalidIdException("Invalid route ID!");

        var forwardStops = route.Stops
            .Where(s => s.Direction == RouteDirection.Forward)
            .OrderBy(s => s.StopIndex)
            .ToList();

        if (!forwardStops.Any())
            throw new ApplicationException("Cannot generate a return route because no forward stops have been defined yet!");

        route.ClearReturnStops();

        int newIndex = 0;
        for (int i = forwardStops.Count - 1; i >= 0; i--)
        {
            var targetStop = forwardStops[i];

            route.AddStop(targetStop.LocationId, newIndex, RouteDirection.Return);
            newIndex++;
        }

        _uow.Routes.Update(route);
        await _uow.SaveChangesAsync();
    }
}
