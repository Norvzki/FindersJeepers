public class Jeepney : AggregateRoot
{
    public int Id { get; private set; } // primary key
    public string PlateNumber { get; private set; } // alternate key
    public string BodyNumber { get; private set; } // alternate key
    public int Capacity { get; private set; }
    public int RouteId { get; private set; }
    private readonly List<JeepneyDriver> _drivers = new();
    public IReadOnlyCollection<JeepneyDriver> Drivers => _drivers;
    private Jeepney()
    {
         
    }

    public static Jeepney Create(string plateNumber, string bodyNumber, int capacity, int routeId)
    {
        if (string.IsNullOrEmpty(plateNumber)) throw new DomainException("Plate number cannot be empty.");
        if (string.IsNullOrEmpty(bodyNumber)) throw new DomainException("Body number cannot be empty.");
        if (capacity == 0) throw new DomainException("Capacity cannot be zero!");
        if (routeId < 1) throw new DomainException("Invalid Route ID!");

        return new Jeepney
        {
            PlateNumber = plateNumber,
            BodyNumber = bodyNumber,
            Capacity = capacity,
            RouteId = routeId
        };

    }

    public void AssignDriver(int driverId)
    {
        if (driverId < 1) throw new DomainException("Invalid driver id!");

        bool alreadyAssigned = _drivers.Any(d => d.DriverId == driverId && d.UnassignedAt == null);
        if (alreadyAssigned) throw new DomainException("This driver is already assigned to this jeepney.");


        _drivers.Add(JeepneyDriver.Create(this.Id, driverId));

    }

    public void RemoveDriver(int driverId)
    {
        if (driverId < 1) throw new DomainException("Invalid driver id!");

        var driver = _drivers.FirstOrDefault(x => x.DriverId == driverId && x.UnassignedAt == null);
        if (driver == null) throw new DomainException("This driver does not drive this jeep!");

        driver.Deactivate();
    }
    public bool IsADriver(int driverId) => _drivers.Any(x => x.DriverId == driverId && x.UnassignedAt == null);


}