public class RouteRepository : Repository<Route>, IRouteRepository
{
    public RouteRepository(MyDbContext context) : base(context)
    {
    }
}

public interface IRouteRepository : IRepository<Route>
{

}
