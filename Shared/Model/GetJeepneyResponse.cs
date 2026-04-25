public class GetJeepneyResponse
{
    public int Id { get; set; }
    public string PlateNumber { get; set; } = string.Empty;
    public string BodyNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int DriverCount { get; set; } = new();
    public int TripCount { get; set; } = new();
    public string RouteCode { get; set; } = string.Empty;
}
