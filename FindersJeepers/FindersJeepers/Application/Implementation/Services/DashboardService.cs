using Microsoft.EntityFrameworkCore;
using System.Data;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _uow;

    public DashboardService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
    {
        return new DashboardSummaryDto
        {
            TotalJeepneys = await GetTotalJeepneysAsync(),
            TotalDrivers = await GetTotalDriversAsync(),
            TotalLocations = await GetTotalLocationsAsync(),
            TotalRoutes = await GetTotalRoutesAsync(),
            ActiveTrips = await GetActiveTripsAsync(),
            WaitingTrips = await GetWaitingTripsAsync(),
            CompletedTripsToday = await GetCompletedTripsTodayAsync(),
            AveragePassengerLoadPercent = await GetAveragePassengerLoadAsync(),
            TripsByRoute = await GetTripsByRouteAsync(),
            RecentTrips = await GetRecentTripsAsync()
        };
    }


    public Task<int> GetTotalJeepneysAsync() =>
        _uow.Jeepneys.Get().Where(j => !j.IsDeleted).CountAsync();

    public Task<int> GetTotalDriversAsync() =>
        _uow.Drivers.Get().Where(d => !d.IsDeleted).CountAsync();

    public Task<int> GetTotalLocationsAsync() =>
        _uow.Locations.Get().Where(l => !l.IsDeleted).CountAsync();

    public Task<int> GetTotalRoutesAsync() =>
        _uow.Routes.Get().Where(r => !r.IsDeleted).CountAsync();

    // ── Trip stats ─────────────────────────────────────────────────────────────

    public Task<int> GetActiveTripsAsync() =>
        _uow.Trips.Get()
            .Where(t => !t.IsDeleted && t.Status == TripStatus.OnGoing)
            .CountAsync();

    public Task<int> GetWaitingTripsAsync() =>
        _uow.Trips.Get()
            .Where(t => !t.IsDeleted && t.Status == TripStatus.Waiting)
            .CountAsync();

    public Task<int> GetCompletedTripsTodayAsync()
    {
        var todayUtc = DateTime.UtcNow.Date;
        return _uow.Trips.Get()
            .Where(t => !t.IsDeleted
                     && t.Status == TripStatus.Completed
                     && t.ArrivalTime.HasValue
                     && t.ArrivalTime.Value.Date == todayUtc)
            .CountAsync();
    }

    /// <summary>
    /// Average occupancy across all Departure logs for completed trips,
    /// expressed as a percentage of capacity.
    /// </summary>
    public async Task<double> GetAveragePassengerLoadAsync()
    {
        var completedTrips = _uow.Trips.Get()
            .Where(t => !t.IsDeleted && t.Status == TripStatus.Completed);

        // Flatten into TripLog rows that have a positive capacity
        var loads = await completedTrips
            .SelectMany(t => t.Logs)
            .Where(l => l.EventType == TripLogType.Departure && l.Capacity > 0)
            .Select(l => (double)l.PassengerCount / l.Capacity * 100)
            .ToListAsync();

        return loads.Count == 0 ? 0 : Math.Round(loads.Average(), 1);
    }

    // ── Chart data ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Total trips (all time, non-deleted) grouped by route — top 8 routes.
    /// </summary>
    public async Task<List<RouteTripsDto>> GetTripsByRouteAsync()
    {
        var trips = _uow.Trips.Get().Where(t => !t.IsDeleted);
        var routes = _uow.Routes.Get().Where(r => !r.IsDeleted);

        return await (from t in trips
                      join r in routes on t.RouteId equals r.Id
                      group t by r.RouteCode into g
                      orderby g.Count() descending
                      select new RouteTripsDto
                      {
                          RouteCode = g.Key,
                          TripCount = g.Count()
                      })
                      .Take(8)
                      .ToListAsync();
    }

    /// <summary>
    /// Last 5 trips ordered by most recent departure, with driver + jeepney info.
    /// </summary>
    public async Task<List<RecentTripDto>> GetRecentTripsAsync()
    {
        var trips = _uow.Trips.Get().Where(t => !t.IsDeleted);
        var drivers = _uow.Drivers.Get();
        var jeepneys = _uow.Jeepneys.Get();
        var routes = _uow.Routes.Get();

        return await (from t in trips
                      join d in drivers on t.DriverId equals d.Id
                      join j in jeepneys on t.JeepneyId equals j.Id
                      join r in routes on t.RouteId equals r.Id
                      orderby t.DepartureTime descending
                      select new RecentTripDto
                      {
                          TripId = t.Id,
                          DriverName = d.FirstName + " " + d.LastName,
                          JeepneyPlate = j.PlateNumber,
                          RouteCode = r.RouteCode,
                          Status = t.Status.ToString(),
                          DepartureTime = t.DepartureTime,
                          ArrivalTime = t.ArrivalTime
                      })
                      .Take(5)
                      .ToListAsync();
    }
}