public class Jeepney : AggregateRoot
{
    public int Id { get; private set; }
    public string PlateNumber { get; private set; }
    public string BodyNumber { get; private set; }
    public int Capacity { get; private set; }
    public int DriverId { get; private set; }
    public int RouteId { get; private set; }
    private Jeepney()
    {
         
    }

    public static Jeepney Create(string plateNumber, string bodyNumber, int capacity, int driverId, int routeId)
    {
        if (plateNumber == null) throw new ArgumentNullException();
        if (bodyNumber == null) throw new ArgumentNullException();
        if (string.IsNullOrEmpty(plateNumber)) throw new DomainException("Plate number cannot be empty.");
        if (string.IsNullOrEmpty(bodyNumber)) throw new DomainException("Body number cannot be empty.");
        if (capacity == 0) throw new DomainException("Capacity cannot be zero!");
        if (driverId < 1) throw new DomainException("Invalid Driver ID!");
        if (routeId < 1) throw new DomainException("Invalid Route ID!");

        return new Jeepney
        {
            PlateNumber = plateNumber,
            BodyNumber = bodyNumber,
            Capacity = capacity,
            DriverId = driverId,
            RouteId = routeId
        };
    }

    public void Hire()
    {
        // raise event JeepneyHireEvent // allows the creation of Trips.
    }
    public void Unhire()
    {
        // raise event JeepneyUnhireEvent // turns the latest trip into "unavailable" if possible. Stops the creation of trips.
        // This event will only activate if there are no ongoing trips for this jeep.
    }
}
