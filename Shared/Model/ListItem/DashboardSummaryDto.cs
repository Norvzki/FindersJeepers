public class DashboardSummaryDto
{
    // Core counts
    public int TotalJeepneys { get; set; }
    public int TotalDrivers { get; set; }
    public int TotalLocations { get; set; }
    public int TotalRoutes { get; set; }

    // Trip stats
    public int ActiveTrips { get; set; }
    public int WaitingTrips { get; set; }
    public int CompletedTripsToday { get; set; }
    public double AveragePassengerLoadPercent { get; set; }

    // Chart & table data
    public List<RouteTripsDto> TripsByRoute { get; set; } = new();
    public List<RecentTripDto> RecentTrips { get; set; } = new();
}

public class RouteTripsDto
{
    public string RouteCode { get; set; } = string.Empty;
    public int TripCount { get; set; }
}

public class RecentTripDto
{
    public int TripId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public string JeepneyPlate { get; set; } = string.Empty;
    public string RouteCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? DepartureTime { get; set; }
    public DateTime? ArrivalTime { get; set; }
}