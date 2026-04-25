public record CreateDriverRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public DateTime DateHired { get; set; }
}

public class CreateRouteRequest
{
    public string RouteCode { get; set; }
    public int StartLocation { get; set; }
    public int EndLocation { get; set; }
}