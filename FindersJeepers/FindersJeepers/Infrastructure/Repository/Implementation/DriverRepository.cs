
using Microsoft.EntityFrameworkCore;
using static MudBlazor.CategoryTypes;

public class DriverRepository : Repository<Driver>, IDriverRepository
{
    public DriverRepository(MyDbContext context) : base(context)
    {
    }

    public override IQueryable<Driver> Get(FetchOptions? options = null)
    {
            if (options == FetchOptions.IncludeDeleted)
                return _context.Drivers.AsQueryable();

            return _context.Drivers.Where(x=>x.IsDeleted == false).AsQueryable();
    }

    //public async Task<List<Driver>> GetByActiveJeepneyAsync(int jeepneyId)
    //  => await _set.Where(driver => _context.Jeepneys
    //        .Where(x => x.Drivers.Any(d => d.DriverId == driver.Id && d.UnassignedAt == null)))
    //        .ToListAsync();
}
