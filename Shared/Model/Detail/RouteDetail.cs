public class RouteDetail
{
    public int Id { get; set; }
    public string RouteCode { get; set; } = string.Empty;
    public LocationDto LocationStart { get; set; } = new();
    public LocationDto LocationEnd { get; set; } = new();
    public List<RouteStopDto> Stops { get; set; } = new();
    public List<RouteStopDto> ReturnStops { get; set; } = new();
    public List<JeepneySummary> AssignedJeepneys { get; set; } = new();
}
