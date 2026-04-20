public class TripRepository : Repository<Trip>, ITripRepository
{
    public TripRepository(MyDbContext context) : base(context)
    {
    }
}

public interface ITripRepository : IRepository<Trip>
{

}