public class Route : AggregateRoot
{
    public int Id { get; private set; }
    public string RouteCode { get; private set; }
    public int LocationStartId { get; private set; }
    public int LocationEndId { get; private set; }

    private readonly List<RouteStop> _stops = new List<RouteStop>();
    public IReadOnlyCollection<RouteStop> Stops => _stops;

    private Route()
    {
         
    }
    public static Route Create(string routeCode, int locationStartId, int locationEndId)
    {
        if (string.IsNullOrEmpty(routeCode)) throw new DomainException("Route Code cannot be empty!");
        if (!IdValidator.ValidateId(locationEndId)) throw new DomainException("Location End ID is invalid!");
        if (!IdValidator.ValidateId(locationStartId)) throw new DomainException("Location Start ID is invalid!");

        return new Route
        {
            RouteCode = routeCode,
            LocationStartId = locationStartId,
            LocationEndId = locationEndId,
        };

    }

    public void AddStop(int locationId, int index)
    {
        if (locationId < 1) throw new DomainException("Invalid location id!");

        _stops.Add(RouteStop.Create(this.Id, locationId, index));
    }
    public void ClearStops() => _stops.Clear(); // might wanna raise domain event here

}
