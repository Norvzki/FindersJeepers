
public interface IDriverService
{
    Task CreateAsync(CreateDriverRequest request);
    Task UpdateAsync(UpdateDriverRequest request);
    Task DeleteAsync(int driverId);
    Task<List<GetDriverResponse>> GetAsync(int pageNumber = -1, int pageSize = -1);
    Task<GetDriverDetailResponse> GetByIdAsync(int driverId);
}
