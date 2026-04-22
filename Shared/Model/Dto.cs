
public class GetDriverResponse
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public DateTime DateHired { get; set; }
}

public class GetJeepneyResponse
{
    public int Id { get; set; }
    public string PlateNumber { get; set; } = string.Empty;
    public string BodyNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public string RouteCode { get; set; } = string.Empty;
}

public class GetRouteResponse
{
    public int Id { get; set; }
    public string RouteCode { get; set; } = string.Empty;
    public string LocationStart { get; set; } = string.Empty;
    public string LocationEnd { get; set; } = string.Empty;
    public List<RouteStopResponse> Stops { get; set; } = new();
}

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

public class GetDriverDetailResponse
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public DateTime DateHired { get; set; }
    public JeepneySummaryResponse? AssignedJeepney { get; set; }
    public List<TripSummaryResponse> TripHistory { get; set; } = new();
}

public class GetJeepneyDetailResponse
{
    public int Id { get; set; }
    public string PlateNumber { get; set; } = string.Empty;
    public string BodyNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string RouteCode { get; set; } = string.Empty;
    public string DriverName { get; set; } = string.Empty;
    public int DriverId { get; set; }
    public string CurrentStatus { get; set; } = string.Empty;
    public TripSummaryResponse? CurrentTrip { get; set; }
    public List<TripSummaryResponse> PastTrips { get; set; } = new();
}

public class GetRouteDetailResponse
{
    public int Id { get; set; }
    public string RouteCode { get; set; } = string.Empty;
    public string LocationStart { get; set; } = string.Empty;
    public string LocationEnd { get; set; } = string.Empty;
    public List<RouteStopResponse> Stops { get; set; } = new();
    public List<JeepneySummaryResponse> AssignedJeepneys { get; set; } = new();
}


public class JeepneySummaryResponse
{
    public int Id { get; set; }
    public string PlateNumber { get; set; } = string.Empty;
    public string BodyNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string RouteCode { get; set; } = string.Empty;
    public string DriverName { get; set; } = string.Empty;
}

public class TripSummaryResponse
{
    public int Id { get; set; }
    public string RouteCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? DepartureTime { get; set; }
    public DateTime? ArrivalTime { get; set; }
    public int LogCount { get; set; }
}

public class RouteStopResponse
{
    public int LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public int StopIndex { get; set; }
}

public class GetTripLogResponse
{
    public string StopName { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public int PassengerCount { get; set; }
    public DateTime Timestamp { get; set; }
}

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