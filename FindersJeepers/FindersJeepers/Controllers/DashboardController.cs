using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/dashboard")]

public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }
    [HttpGet("summary")]
    public async Task<IActionResult> GetDashboardSummary()
    {
        var result = await _dashboardService.GetDashboardSummaryAsync();
        return Ok(result);
    }
    [HttpGet("jeepneys")]
    public async Task<IActionResult> GetTotalJeepneys()
    {
        var result = await _dashboardService.GetTotalJeepneysAsync();
        return Ok(result);
    }
}

