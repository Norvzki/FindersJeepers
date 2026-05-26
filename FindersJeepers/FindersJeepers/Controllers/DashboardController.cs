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

}

[ApiController]
[Route("api/v1/presentation")]
public class PresentationController : ControllerBase
{
    private readonly IGenerationService _generationService;

    private readonly ISimulationService _simulationService;

    public PresentationController(IGenerationService generationService, ISimulationService simulationService)
    {
        _generationService = generationService;
        _simulationService = simulationService;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate()
    {
        await _generationService.GenerateAsync();
        return Ok();
    }

    [HttpPost("simulate")]
    public async Task<IActionResult> Simulate()
    {
        await _simulationService.RunAsync();
        return Ok();
    }

}