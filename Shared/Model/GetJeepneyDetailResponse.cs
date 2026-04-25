public class GetJeepneyDetailResponse
{
    public int Id { get; set; }
    public string PlateNumber { get; set; } = string.Empty;
    public string BodyNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string RouteCode { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
    public List<GetDriverSummaryResponse> AssignedDrivers { get; set; } = new();
    public GetTripSummaryResponse? CurrentTrip { get; set; }
    public List<GetTripSummaryResponse> PastTrips { get; set; } = new();
}
