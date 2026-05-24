public class LocationRepository : Repository<Location>, ILocationRepository
{
    public LocationRepository(MyDbContext context) : base(context)
    {
    }

    public override IQueryable<Location> Get(FetchOptions? options = null)
    {
        if (options == FetchOptions.IncludeDeleted)
            return _context.Locations.AsQueryable();

        return _context.Locations.Where(x => x.IsDeleted == false).AsQueryable();
    }
}
