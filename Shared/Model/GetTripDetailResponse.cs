public class GetTripDetailResponse
{
    public int Id { get; set; }
    public int JeepneyId { get; set; }
    public string PlateNumber { get; set; } = string.Empty;
    public int RouteId { get; set; }
    public string RouteCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public DateTime? DepartureTime { get; set; }
    public DateTime? ArrivalTime { get; set; }
    public List<GetTripLogResponse> Logs { get; set; } = new();
}
