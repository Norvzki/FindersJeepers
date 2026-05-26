using System.ComponentModel.DataAnnotations.Schema;

public class Route : AggregateRoot
{
    public int Id { get; private set; }
    public string RouteCode { get; private set; }
    public int LocationStartId { get; private set; }
    public int LocationEndId { get; private set; }

    public bool IsDeleted { get; private set; } = false;

    public void Delete()
    {
        if (IsDeleted) throw new DomainException("This is entity deleted!");
        IsDeleted = true;
    }


    private readonly List<RouteStop> _stops = new();
    public IReadOnlyCollection<RouteStop> Stops => _stops;

    [NotMapped]
    public IEnumerable<RouteStop> ForwardStops =>
        _stops.Where(s => s.Direction == RouteDirection.Forward).OrderBy(s => s.StopIndex);

    [NotMapped]
    public IEnumerable<RouteStop> ReturnStops =>
        _stops.Where(s => s.Direction == RouteDirection.Return).OrderBy(s => s.StopIndex);

    private Route() { }

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

    public void UpdateInformation(string routeCode, int locationStartId, int locationEndId)
    {
        if (string.IsNullOrEmpty(routeCode)) throw new DomainException("Route Code cannot be empty!");
        if (!IdValidator.ValidateId(locationEndId)) throw new DomainException("Location End ID is invalid!");
        if (!IdValidator.ValidateId(locationStartId)) throw new DomainException("Location Start ID is invalid!");

        RouteCode = routeCode;
        LocationStartId = locationStartId;
        LocationEndId = locationEndId;
    }

    public void AddStop(int locationId, int index, RouteDirection direction)
    {
        if (locationId < 1) throw new DomainException("Invalid location id!");
        _stops.Add(RouteStop.Create(this.Id, locationId, index, direction));
    }


    public void GenerateReturnStopsFromForward()
    {
        var forwardStops = ForwardStops.ToList();
        if (!forwardStops.Any())
            throw new DomainException("Cannot generate return stops — no forward stops defined.");

        ClearReturnStops();

        var reversed = forwardStops
            .OrderByDescending(s => s.StopIndex)
            .Select((s, i) => RouteStop.Create(this.Id, s.LocationId, i, RouteDirection.Return))
            .ToList();

        _stops.AddRange(reversed);
    }

    public void ClearStops() => _stops.RemoveAll(s => s.Direction == RouteDirection.Forward);
    public void ClearReturnStops() => _stops.RemoveAll(s => s.Direction == RouteDirection.Return);
}