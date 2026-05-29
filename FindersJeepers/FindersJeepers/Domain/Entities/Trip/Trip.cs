public class Trip : AggregateRoot
{
    public int Id { get; private set; } // Pk
    public int DriverId { get; private set; } // Composite Key
    public int JeepneyId { get; private set; } // Composite Key
    public int RouteId { get; private set; } // We dont really need this, right? (We do)
    public RouteDirection Direction { get; private set; }

    public DateTime? DepartureTime {  get; private set; }
    public DateTime? ArrivalTime { get; private set; }
    public TripStatus Status { get; private set; }

    private List<TripLog> _logs = new List<TripLog>();
    public IReadOnlyCollection<TripLog> Logs => _logs;


    public bool IsDeleted { get; private set; } = false;
    public void Delete()
    {
        if (IsDeleted) throw new DomainException("This is entity deleted!");
        IsDeleted = true;
    }

    private Trip()
    {
    }
    public static Trip Create(int driverId, int jeepneyId, int routeId, RouteDirection routeDirection)
    {
        if(!IdValidator.ValidateId(jeepneyId)) throw new DomainException("Invalid jeepney ID!");
        if (!IdValidator.ValidateId(routeId)) throw new DomainException("Invalid jeepney ID!");

        return new Trip
        {
            DriverId = driverId,
            JeepneyId = jeepneyId,
            RouteId = routeId,
            DepartureTime = null,
            ArrivalTime = null,
            Status = TripStatus.Waiting,
            Direction = routeDirection
        };
    }

    public void StartTrip()
    {
        if (Status == TripStatus.OnGoing) throw new DomainException("Trip has already started and is ongoing!");
        if (Status == TripStatus.Completed) throw new DomainException("You cannot start an already completed trip!");
        if (Status == TripStatus.Aborted) throw new NotImplementedException("You cannot start a completed trip!");

        DepartureTime = DateTime.UtcNow;
        Status = TripStatus.OnGoing;
    }

    public void CompleteTrip(bool isAborted = false)
    {
        if (Status != TripStatus.OnGoing)
            throw new DomainException("Trip must be ongoing to complete.");
        
            Status = TripStatus.Completed;

        if (isAborted)
            Status = TripStatus.Aborted;

        ArrivalTime = DateTime.UtcNow;
    }

    public TripLog? GetLatestLog() => Logs.FirstOrDefault();

    public void LogStopEvent(int locationId, int passengerCount, int capacity, TripLogType logType)
    {
        // i arrive at ayala with N people. When you arrive, CLASSIFY ONLY THOSE WHO GET OFF THE JEEP.
        // i depart from ayala with N people. When departing, CLASSIFY ONLY THOSE WHO HAVE GET ON THE JEEP.

        if (Status != TripStatus.OnGoing) throw new DomainException("Trip has not started yet!");
        var log = TripLog.Create(this.Id, locationId, passengerCount,capacity, logType);
        _logs.Add(log);
        // Event: if this log is Route.LocationStopId then complete this trip.
        // Event: if passengerCount is equal to Jeepney.Capacity, Jeepney.Status will be Status.Full
        // Event: if passengerCount is higher than Jeepner.Capacity, Jeepney.Status will be Status.Overloaded
    }



}
