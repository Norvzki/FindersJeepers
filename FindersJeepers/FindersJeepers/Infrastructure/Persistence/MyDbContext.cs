
using Microsoft.EntityFrameworkCore;

public class MyDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {
         
    }

    public DbSet<Jeepney> Jeepneys {  get; set; }
    public DbSet<JeepneyDriver> JeepneyDrivers { get; set; }
    public DbSet<Driver> Drivers { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Trip> Trips { get; set; }
    public DbSet<Route> Routes { get; set; }

    public DbSet<TripLog> TripLogs { get; set; }
    public DbSet<RouteStop> RouteStops { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --------------------
        // LOCATIONS
        // --------------------
        modelBuilder.Entity<Location>().HasData(
            new { Id = 1, Name = "Ayala Center Cebu", Description = "Major business district" },
            new { Id = 2, Name = "SM City Cebu", Description = "Shopping mall hub" },
            new { Id = 3, Name = "IT Park", Description = "Tech and BPO hub" },
            new { Id = 4, Name = "Carbon Market", Description = "Public market" },
            new { Id = 5, Name = "Colon Street", Description = "Historic downtown" },
            new { Id = 6, Name = "Banilad", Description = "Residential area" }
        );

        // --------------------
        // DRIVERS
        // --------------------
        modelBuilder.Entity<Driver>().HasData(
            new { Id = 1, FirstName = "Juan", LastName = "Dela Cruz", LicenseNumber = "LIC1001", ContactNumber = "09171234567", DateHired = DateTime.UtcNow.AddYears(-5) },
            new { Id = 2, FirstName = "Pedro", LastName = "Santos", LicenseNumber = "LIC1002", ContactNumber = "09181234567", DateHired = DateTime.UtcNow.AddYears(-3) },
            new { Id = 3, FirstName = "Mario", LastName = "Reyes", LicenseNumber = "LIC1003", ContactNumber = "09191234567", DateHired = DateTime.UtcNow.AddYears(-2) },
            new { Id = 4, FirstName = "Jose", LastName = "Garcia", LicenseNumber = "LIC1004", ContactNumber = "09201234567", DateHired = DateTime.UtcNow.AddYears(-1) },
            new { Id = 5, FirstName = "Luis", LastName = "Torres", LicenseNumber = "LIC1005", ContactNumber = "09211234567", DateHired = DateTime.UtcNow.AddMonths(-6) }
        );

        // --------------------
        // ROUTES
        // --------------------
        modelBuilder.Entity<Route>().HasData(
            new { Id = 1, RouteCode = "R1", LocationStartId = 1, LocationEndId = 2 },
            new { Id = 2, RouteCode = "R2", LocationStartId = 3, LocationEndId = 4 },
            new { Id = 3, RouteCode = "R3", LocationStartId = 2, LocationEndId = 5 }
        );

        // --------------------
        // ROUTE STOPS
        // --------------------
        modelBuilder.Entity<RouteStop>().HasData(
            new { Id = 1, RouteId = 1, LocationId = 1, StopIndex = 0 },
            new { Id = 2, RouteId = 1, LocationId = 6, StopIndex = 1 },
            new { Id = 3, RouteId = 1, LocationId = 2, StopIndex = 2 },

            new { Id = 4, RouteId = 2, LocationId = 3, StopIndex = 0 },
            new { Id = 5, RouteId = 2, LocationId = 6, StopIndex = 1 },
            new { Id = 6, RouteId = 2, LocationId = 4, StopIndex = 2 },

            new { Id = 7, RouteId = 3, LocationId = 2, StopIndex = 0 },
            new { Id = 8, RouteId = 3, LocationId = 1, StopIndex = 1 },
            new { Id = 9, RouteId = 3, LocationId = 5, StopIndex = 2 }
        );

        // --------------------
        // JEEPNEYS
        // --------------------
        modelBuilder.Entity<Jeepney>().HasData(
            new { Id = 1, PlateNumber = "ABC123", BodyNumber = "J001", Capacity = 16, RouteId = 1 },
            new { Id = 2, PlateNumber = "DEF456", BodyNumber = "J002", Capacity = 18, RouteId = 2 },
            new { Id = 3, PlateNumber = "GHI789", BodyNumber = "J003", Capacity = 20, RouteId = 3 }
        );

        // --------------------
        // TRIPS
        // --------------------
        modelBuilder.Entity<Trip>().HasData(
            new { Id = 1, DriverId = 1, JeepneyId = 1, RouteId = 1, DepartureTime = (DateTime?)null, ArrivalTime = (DateTime?)null, Status = TripStatus.Waiting },
            new { Id = 2, DriverId = 2, JeepneyId = 2, RouteId = 2, DepartureTime = (DateTime?)null, ArrivalTime = (DateTime?)null, Status = TripStatus.Waiting },
            new { Id = 3, DriverId = 3, JeepneyId = 3, RouteId = 3, DepartureTime = (DateTime?)null, ArrivalTime = (DateTime?)null, Status = TripStatus.Waiting }
        );

        // --------------------
        // TRIP LOGS
        // --------------------
        modelBuilder.Entity<TripLog>().HasData(
            new { Id = 1, TripId = 1, StopId = 1, PassengerCount = 5, EventType = TripLogType.Arrival, TimeStamp = DateTime.UtcNow },
            new { Id = 2, TripId = 1, StopId = 2, PassengerCount = 3, EventType = TripLogType.Departure, TimeStamp = DateTime.UtcNow },

            new { Id = 3, TripId = 2, StopId = 4, PassengerCount = 10, EventType = TripLogType.Arrival, TimeStamp = DateTime.UtcNow },
            new { Id = 4, TripId = 2, StopId = 5, PassengerCount = 6, EventType = TripLogType.Departure, TimeStamp = DateTime.UtcNow },

            new { Id = 5, TripId = 3, StopId = 7, PassengerCount = 8, EventType = TripLogType.Arrival, TimeStamp = DateTime.UtcNow }
        );
    }
}