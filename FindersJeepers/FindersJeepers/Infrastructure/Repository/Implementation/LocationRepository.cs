public class LocationRepository : Repository<Location>, ILocationRepository
{
    public LocationRepository(MyDbContext context) : base(context)
    {
    }

    public override IQueryable<Location> Get(FetchOptions? options = null) => _context.Locations.Where(x => x.IsDeleted == false).AsQueryable();
}
