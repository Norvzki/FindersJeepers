
public interface IRouteService
    {
    Task<List<RouteDto>> GetRoutesAsync();
    Task CreateRouteAsync(CreateRouteRequest req);
    Task<RouteDetail> GetDetailAsync(int routeId);
    Task AddRouteStopsAsync(AddRouteStopRequest req);
    Task DeleteAsync(int routeId);
    Task UpdateAsync(UpdateRouteRequest req);
}
