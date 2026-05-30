

using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Runtime.CompilerServices;

public class JeepFinderService : IJeepFinderService
{
    private readonly IUnitOfWork _uow;

    public JeepFinderService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<List<FoundJeepDto>> FindJeepsAsync(int locationId)
    {
        var routes = await _uow.Routes.GetByLocationAsync(locationId);

        if (!routes.Any())
            return new List<FoundJeepDto>();

        var results = new Dictionary<int, FoundJeepDto>();

        foreach (var route in routes)
        {
            var activeTrips = await _uow.Trips.GetActiveTripsOnRouteAsync(route.Id);

            foreach (var trip in activeTrips)
            {
                var orderedStops = trip.Direction == RouteDirection.Forward
                    ? route.ForwardStops.ToList()
                    : route.ReturnStops.ToList();

                var passengerStop = orderedStops.FirstOrDefault(s => s.LocationId == locationId);
                if (passengerStop == null)
                    continue;

                var latestLog = trip.Logs
                    .OrderByDescending(l => l.TimeStamp)
                    .FirstOrDefault();

                int jeepStopIndex = -1;
                if (latestLog != null)
                {
                    var currentStop = orderedStops.FirstOrDefault(s => s.LocationId == latestLog.LocationId);
                    if (currentStop != null)
                        jeepStopIndex = currentStop.StopIndex;
                }

                if (jeepStopIndex >= passengerStop.StopIndex)
                    continue;

                int stopsAway = passengerStop.StopIndex - jeepStopIndex;

                var jeepney = await _uow.Jeepneys.GetByIdAsync(trip.JeepneyId);
                if (jeepney == null)
                    continue;

                var dto = new FoundJeepDto
                {
                    Jeepney = new JeepneyDto
                    {
                        Id = jeepney.Id,
                        PlateNumber = jeepney.PlateNumber,
                        BodyNumber = jeepney.BodyNumber,
                        Capacity = jeepney.Capacity,
                        DriverCount = jeepney.Drivers.Count(d => d.UnassignedAt == null),
                        TripCount = (await _uow.Trips.GetActiveTripsOnRouteAsync(route.Id))
                                          .Count(t => t.JeepneyId == jeepney.Id),
                        RouteCode = route.RouteCode
                    },
                    StopsAwayCount = stopsAway
                };

                if (!results.TryGetValue(jeepney.Id, out var existing) ||
                    stopsAway < existing.StopsAwayCount)
                {
                    results[jeepney.Id] = dto;
                }
            }
        }

        return results.Values
            .OrderBy(r => r.StopsAwayCount)
            .ToList();
    }
}