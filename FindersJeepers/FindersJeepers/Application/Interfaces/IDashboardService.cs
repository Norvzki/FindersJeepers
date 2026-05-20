
public interface IDashboardService
{
    Task<DashboardSummaryDto> GetDashboardSummaryAsync();
    Task<int> GetTotalJeepneysAsync();
    Task<int> GetTotalDriversAsync();
    Task<int> GetTotalLocationsAsync();
    Task<int> GetTotalRoutesAsync();
}