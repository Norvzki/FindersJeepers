public class TripSummary
{
    public int Id { get; set; }
    public string RouteCode { get; set; } = string.Empty;
    public string JeepneyPlateNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public DriverSummary Driver { get; set; }
    public DateTime? DepartureTime { get; set; }
    public DateTime? ArrivalTime { get; set; }
    public int LogCount { get; set; }
}
