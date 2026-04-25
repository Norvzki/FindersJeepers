public class GetDriverDetailResponse
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public DateTime DateHired { get; set; }
    public List<GetJeepneySummaryResponse>? AssignedJeepneys { get; set; }
    public List<GetTripSummaryResponse> TripHistory { get; set; } = new();
}
