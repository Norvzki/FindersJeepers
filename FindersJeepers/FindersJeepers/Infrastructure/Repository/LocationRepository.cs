public class LocationRepository : Repository<Location>, ILocationRepository
{
    public LocationRepository(MyDbContext context) : base(context)
    {
    }
}

public interface ILocationRepository : IRepository<Location>
{

}