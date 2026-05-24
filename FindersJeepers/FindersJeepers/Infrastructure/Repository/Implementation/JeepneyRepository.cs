
using Microsoft.EntityFrameworkCore;

public class JeepneyRepository : Repository<Jeepney>, IJeepneyRepository
{
    public JeepneyRepository(MyDbContext context) : base(context)
    {
    }
    public override async Task<Jeepney> GetByIdAsync(int id)
    {
        return await _set.Include(x => x.Drivers).FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Jeepney>> GetByDriverAsync(int driverId)
        => await _context.Jeepneys
            .Include(j => j.Drivers)
            .Where(j => j.Drivers.Any(d => d.DriverId == driverId && d.UnassignedAt == null))
            .ToListAsync();

    public async Task<List<Jeepney>> GetByRouteAsync(int routeId) => await _context.Jeepneys.Where(x=>x.RouteId  == routeId).ToListAsync();

    public override IQueryable<Jeepney> Get(FetchOptions? options = null) => _context.Jeepneys.Where(x => x.IsDeleted == false).AsQueryable();


    //public async Task<List<Jeepney>> GetStandbyJeepsOfDriver(int driverId)
    //{
    //    var drivers = 
    //    return await _set.Where(j => _context.Trips.Any(x => x.JeepneyId == j.Id && x.Status == TripStatus.Waiting))
    //}
}
