
using Microsoft.EntityFrameworkCore;

public class RouteRepository : Repository<Route>, IRouteRepository
{
    public RouteRepository(MyDbContext context) : base(context)
    {
    }
    public override async Task<Route> GetByIdAsync(int id) => await _set.Include(x => x.Stops).FirstOrDefaultAsync(x => x.Id == id);

    public override IQueryable<Route> Get(FetchOptions? options = null)
    {
        if (options == FetchOptions.IncludeDeleted)
            return _context.Routes.AsQueryable();

        return _context.Routes.Where(x => x.IsDeleted == false).AsQueryable();
    }

    public async Task<List<Route>> GetByLocationAsync(int locationId)
        => await _context.Routes
            .Include(r => r.Stops)
            .Where(r => r.LocationStartId == locationId ||
                        r.LocationEndId == locationId ||
                        r.Stops.Any(s => s.LocationId == locationId))
            .ToListAsync();

    public async Task<Route?> GetByRouteCodeAsync(string routecode)
        => await _context.Routes
        .Where(r => r.RouteCode == routecode && r.IsDeleted == false)
        .FirstOrDefaultAsync();
}
