public class GetJeepneySummaryResponse
{
    public int Id { get; set; }
    public string PlateNumber { get; set; } = string.Empty;
    public string BodyNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string RouteCode { get; set; } = string.Empty;
    public string DriverName { get; set; } = string.Empty;
}
