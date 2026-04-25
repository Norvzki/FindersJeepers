
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("api/options")]
public class OptionsController : ControllerBase
{
    private readonly IOptionService _optionService;

    public OptionsController(IOptionService optionService)
    {
        _optionService = optionService;
    }

    [HttpGet("drivers/available-for-jeep/{jeepId:int}")]
    public async Task<IActionResult> GetDriversForJeep(int jeepId)
    {
        var result = await _optionService.GetDriversForJeep(jeepId);
        return Ok(result);
    }

    [HttpGet("drivers/available-for-driver/{driverId:int}")]
    public async Task<IActionResult> GetJeepsForDriver(int driverId) {
        var result = await _optionService.GetJeepsForDriver(driverId);
        return Ok(result);
    }

}