
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/api/v1/routes")]
public class RouteController : ControllerBase
{
    private readonly IRouteService _routeService;

    public RouteController(IRouteService routeService)
    {
        _routeService = routeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetRoutes()
    {
        var result = await _routeService.GetRoutes();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRoute([FromBody] CreateRouteRequest request)
    {
        await _routeService.CreateRouteAsync(request);
        return Created();
    }
}
