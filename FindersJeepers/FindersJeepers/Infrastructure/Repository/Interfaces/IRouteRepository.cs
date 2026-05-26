
public interface IRouteRepository : IRepository<Route>
{
    Task<List<Route>> GetByLocationAsync(int locationId);
    Task<Route?> GetByRouteCodeAsync(string routecode);
}
