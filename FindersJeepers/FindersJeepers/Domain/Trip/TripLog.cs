public class TripLog // belongs to trip
{
    public int Id { get; set; } // primary key
    public int TripId { get; private set; } // composite key
    public int LocationId { get; private set; } // location id nalang
    public int PassengerCount { get; private set; }
    public int Capacity { get; private set; }
    public DateTime TimeStamp { get; private set; }
    public TripLogType EventType {  get; private set; }

    private TripLog()
    {
         
    }

    public static TripLog Create(int tripId, int locationId, int passengerCount, int capacity, TripLogType eventType) 
    {
        if (!IdValidator.ValidateId(tripId)) throw new DomainException("Invalid Trip Id!");
        if (!IdValidator.ValidateId(locationId)) throw new DomainException("Invalid Location Id");
        if (passengerCount < 0) throw new DomainException("Passenger count cannot be lower than zero!");

        return new TripLog
        {
            TripId = tripId,
            LocationId = locationId,
            PassengerCount = passengerCount,
            EventType = eventType,
            Capacity = capacity,
            TimeStamp = DateTime.UtcNow
        };
    }
}

