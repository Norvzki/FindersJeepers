public class GetTripResponse
{
    public int Id { get; set; }
    public string PlateNumber { get; set; } = string.Empty;
    public string RouteCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? DepartureTime { get; set; }
    public DateTime? ArrivalTime { get; set; }
    public int LogCount { get; set; }
}
