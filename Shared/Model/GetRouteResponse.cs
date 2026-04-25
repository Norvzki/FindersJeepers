public class GetRouteResponse
{
    public int Id { get; set; }
    public string RouteCode { get; set; } = string.Empty;
    public string LocationStart { get; set; } = string.Empty;
    public string LocationEnd { get; set; } = string.Empty;
    public List<GetRouteStopResponse> Stops { get; set; } = new();
}
