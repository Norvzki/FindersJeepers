using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/jeepfinder/")]
public class FindJeepController : ControllerBase
{
    private readonly IJeepFinderService _findJeepService;
    private readonly ILocationService _locationService;

    public FindJeepController(IJeepFinderService findJeepService, ILocationService locationService)
    {
        _findJeepService = findJeepService;
        _locationService = locationService;
    }

    /// <summary>
    /// GET api/findjeep/{locationId}
    /// Returns all jeepneys heading toward the given stop, sorted nearest-first.
    /// </summary>
    [HttpGet("{locationId:int}")]
    [ProducesResponseType(typeof(List<FoundJeepDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FindJeeps(int locationId)
    {
        if (locationId < 1)
            return BadRequest("Invalid location ID.");

        var results = await _findJeepService.FindJeepsAsync(locationId);
        return Ok(results);
    }
}