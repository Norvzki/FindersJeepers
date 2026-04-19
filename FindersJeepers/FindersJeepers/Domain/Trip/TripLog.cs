public class TripLog // belongs to trip
{
    public int Id { get; set; } // primary key
    public int TripId { get; private set; } // composite key
    public int StopId { get; private set; } 
    public int PassengerCount { get; private set; }
    public DateTime TimeStamp { get; private set; }
    public TripLogType EventType {  get; private set; }

    private TripLog()
    {
         
    }

    public static TripLog Create(int tripId, int stopId, int passengerCount, TripLogType eventType) 
    {
        if (IdValidator.ValidateId(tripId)) throw new DomainException("Invalid Trip Id!");
        if (IdValidator.ValidateId(stopId)) throw new DomainException("Invalid Stop Id");
        if (passengerCount < 0) throw new DomainException("Passenger count cannot be lower than zero!");

        return new TripLog
        {
            TripId = tripId,
            StopId = stopId,
            PassengerCount = passengerCount,
            EventType = eventType,
            TimeStamp = DateTime.UtcNow
        };
    }
}
