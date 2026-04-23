
public interface IDriverService
{
    Task CreateAsync(CreateDriverRequest request);
    Task UpdateAsync(UpdateDriverRequest request);
    Task DeleteAsync(int driverId);
    Task<List<GetDriverResponse>> GetAsync(int pageNumber = -1, int pageSize = -1);
    Task<GetDriverDetailResponse> GetByIdAsync(int driverId);
}

public interface IJeepService
{
    Task CreateAsync(CreateJeepneyRequest request);
    Task UpdateAsync(UpdateJeepneyRequest request);
    Task DeleteAsync(int jeepId);

    Task<List<GetJeepneyResponse>> GetAsync(int pageNumber = -1, int pageSize = -1);
    Task<GetJeepneyDetailResponse> GetByIdAsync(int jeepId);

    Task AssignDriver(int jeepId, int driverId);
}

public interface ITripService
{
    Task<List<GetTripResponse>> GetTrips(int driverId, int jeepId);
}

public interface ILocationService
{

}

public interface IRouteService
{

}
