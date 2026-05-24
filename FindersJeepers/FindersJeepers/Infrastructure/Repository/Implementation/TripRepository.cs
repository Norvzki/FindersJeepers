using Microsoft.EntityFrameworkCore;

public class TripRepository : Repository<Trip>, ITripRepository
{
    public TripRepository(MyDbContext context) : base(context)
    {
    }

    public async Task<Trip?> GetCurrentTripByDriverAsync(int driverId) =>
        await _set.Where(x => x.DriverId == driverId && (x.Status == TripStatus.OnGoing || x.Status == TripStatus.Waiting))
            .FirstOrDefaultAsync();
    public async Task<Trip?> GetCurrentTripByJeepneyAsync(int jeepId) =>
        await _set.Where(x => x.JeepneyId == jeepId && (x.Status == TripStatus.OnGoing || x.Status == TripStatus.Waiting))
            .FirstOrDefaultAsync();

    public async Task<List<Trip>> GetTripsOfJeepAsync(int jeepId) =>
        await _set.Where(x => x.JeepneyId == jeepId).ToListAsync();

    public override Task<Trip> GetByIdAsync(int id) => _context.Trips.Include(x=>x.Logs)
        .FirstOrDefaultAsync(x=>x.Id == id);


    public async Task<List<Trip>> GetActiveTripsOnRouteAsync(int routeId) => await _context.Trips
        .Where(x=>x.RouteId == routeId && x.Status != TripStatus.Completed).ToListAsync();

    public override IQueryable<Trip> Get(FetchOptions? options = null)
    {
        if (options == FetchOptions.IncludeDeleted)
            return _context.Trips.AsQueryable();
        return _context.Trips.Where(x => x.IsDeleted == false).AsQueryable();
    }
}
