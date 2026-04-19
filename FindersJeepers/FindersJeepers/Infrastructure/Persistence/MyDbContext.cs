
using Microsoft.EntityFrameworkCore;

public class MyDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    public DbSet<Jeepney> Jeepneys {  get; set; }
    public DbSet<Driver> Drivers { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Trip> Trips { get; set; }
    public DbSet<Route> Routes { get; set; }

    public DbSet<TripLog> TripLogs { get; set; }
    public DbSet<RouteStop> RouteStops { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
    }
}