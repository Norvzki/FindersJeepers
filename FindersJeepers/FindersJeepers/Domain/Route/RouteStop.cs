public class RouteStop // belongs to route, right?
{
    // PK is Id
    public int Id { get; private set; } // PK
    public int RouteId { get; private set; }  
    public int LocationId { get; private set; } 
    public int StopIndex { get; private set; } // what position this is in the route's stops

    private RouteStop()
    {
         
    }

    public static RouteStop Create(int routeId, int locationId, int stopIndex)
    {
        if (routeId < 1) throw new DomainException("Invalid route id!");
        if (locationId < 1) throw new DomainException("Invalid location id!");
        if (stopIndex < 0) throw new DomainException("Invalid stop index!");

        return new RouteStop
        {
            RouteId = routeId,
            LocationId = locationId,
            StopIndex = stopIndex
        };
    }
}
