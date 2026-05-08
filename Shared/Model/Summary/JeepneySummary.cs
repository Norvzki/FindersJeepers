public class JeepneySummary
{
    public int Id { get; set; }
    public string PlateNumber { get; set; } = string.Empty;
    public string BodyNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string RouteCode { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }          // false = currently on a trip
    public string? CurrentDriverName { get; set; } // who's driving it right now

}

