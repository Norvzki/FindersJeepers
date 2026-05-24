
public interface IJeepneyRepository : IRepository<Jeepney>
{
    Task<List<Jeepney>> GetByDriverAsync(int driverId);
    Task<List<Jeepney>> GetByRouteAsync(int routeId);
}