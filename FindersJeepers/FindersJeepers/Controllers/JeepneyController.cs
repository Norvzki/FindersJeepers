
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/api/v1/jeepneys")]
public class JeepneyController : ControllerBase
{
    private readonly IJeepService jeepService;

    public JeepneyController(IJeepService jeepService)
    {
        this.jeepService = jeepService;
    }

    [HttpGet]
    public async Task<IActionResult> GetJeepneys()
    {
        var result = await jeepService.GetAsync();
        return Ok(result);
    }
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetJeepneyById(int id)
    {
        var result = await jeepService.GetByIdAsync(id);
        return Ok(result);
    }
    [HttpPost]
    public async Task<IActionResult> CreateJeepney([FromBody] CreateJeepneyRequest req)
    {
        await jeepService.CreateAsync(req);
        return Created();
    }

    [HttpPut("")]
    public async Task<IActionResult> AssignDrivers([FromBody] AssignDriversRequest req)
    {
        await jeepService.AssignDriversAsync(req);
        return Ok();
    }
}