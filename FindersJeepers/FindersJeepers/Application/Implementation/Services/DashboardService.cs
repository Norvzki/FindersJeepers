using Microsoft.EntityFrameworkCore;
using System.Data;

public class DashboardService : IDashboardService
{
    // Implement this norvel? :)
    // 3 weeks later... ok ?
    private readonly IUnitOfWork _uow;
    public DashboardService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
    {

        return new DashboardSummaryDto
        {
            TotalJeepneys = await GetTotalJeepneysAsync(),
            TotalDrivers = await GetTotalDriversAsync(),
            TotalLocations = await GetTotalLocationsAsync(),
            TotalRoutes = await GetTotalRoutesAsync()
        };
    }

    public async Task<int> GetTotalJeepneysAsync()
    {
        return await _uow.Jeepneys.Get()
        .Where(j=>!j.IsDeleted)
        .CountAsync();
    }

    public async Task<int> GetTotalDriversAsync()
    {
        return await _uow.Drivers.Get()
        .Where(j => !j.IsDeleted)
        .CountAsync();
    }

    public async Task<int> GetTotalLocationsAsync()
    {
        return await _uow.Locations.Get()
        .Where(j => !j.IsDeleted)
        .CountAsync();
    }

    public async Task<int> GetTotalRoutesAsync()
    {
        return await _uow.Routes.Get()
        .Where(j => !j.IsDeleted)
        .CountAsync();
    }
}

