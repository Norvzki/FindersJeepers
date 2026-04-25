public class GetLocationDetailResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<GetRouteSummaryResponse> Routes { get; set; } = new();
    public List<GetRouteStopOccurrenceResponse> StopOccurrences { get; set; } = new();
}
