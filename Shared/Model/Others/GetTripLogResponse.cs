public class GetTripLogResponse
{
    public string StopName { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public int PassengerCount { get; set; }
    public DateTime Timestamp { get; set; }
}
