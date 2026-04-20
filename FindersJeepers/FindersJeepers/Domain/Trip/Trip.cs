public class Trip : AggregateRoot
{
    public int Id { get; private set; } // Pk
    public int JeepneyId { get; private set; } // Composite Key
    public int RouteId { get; private set; } // We dont really need this, right?

    public DateTime? DepartureTime {  get; private set; }
    public DateTime? ArrivalTime { get; private set; }
    public TripStatus Status { get; private set; }

    private List<TripLog> _logs = new List<TripLog>();
    public IReadOnlyCollection<TripLog> Logs => _logs;

    private Trip()
    {
    }
    public static Trip Create(int jeepneyId, int routeId)
    {
        if(!IdValidator.ValidateId(jeepneyId)) throw new DomainException("Invalid jeepney ID!");
        if (!IdValidator.ValidateId(routeId)) throw new DomainException("Invalid jeepney ID!");

        return new Trip
        {
            JeepneyId = jeepneyId,
            RouteId = routeId,
            DepartureTime = null,
            ArrivalTime = null,
            Status = TripStatus.Waiting
        };
    }

    public void StartTrip()
    {
        if (Status == TripStatus.OnGoing) throw new DomainException("Trip has already started and is ongoing!");
        if (Status == TripStatus.Completed) throw new DomainException("You cannot start an already completed trip!");
        if (Status == TripStatus.Unavailable) throw new NotImplementedException("Not implemented yet.");

        DepartureTime = DateTime.UtcNow;
        Status = TripStatus.OnGoing;
    }

    public void CompleteTrip()
    {
        if (Status != TripStatus.OnGoing)
            throw new DomainException("Trip must be ongoing to complete.");

        Status = TripStatus.Completed;
        ArrivalTime = DateTime.UtcNow;
    }

    public void LogStopEvent(int stopId, int passengerCount, TripLogType logType)
    {
        // i arrive at ayala with N people. When you arrive, CLASSIFY ONLY THOSE WHO GET OFF THE JEEP.
        // i depart from ayala with N people. When departing, CLASSIFY ONLY THOSE WHO HAVE GET ON THE JEEP.

        if (Status != TripStatus.OnGoing) throw new DomainException("Trip has not started yet!");

        var log = TripLog.Create(this.Id, stopId, passengerCount, logType);
        _logs.Add(log);
        // Event: if this log is Route.LocationStopId then complete this trip.
        // Event: if passengerCount is equal to Jeepney.Capacity, Jeepney.Status will be Status.Full
        // Event: if passengerCount is higher than Jeepner.Capacity, Jeepney.Status will be Status.Overloaded
    }



}
