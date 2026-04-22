
public interface IDriverService
{
    Task CreateAsync(CreateDriverRequest request);
    Task UpdateAsync(UpdateDriverRequest request);
    Task DeleteAsync(int driverId);
    Task<List<GetDriverResponse>> GetAsync(int pageNumber, int pageSize);
    Task<GetDriverDetailResponse> GetByIdAsync(int driverId);
    Task<GetDriverDetailResponse> GetDetail(int driverId);
}

public interface IJeepService
{
    Task AssignDriver(int jeepId, int driverId);
    Task GetTrips(int driverId);
}

public interface ITripService
{

}