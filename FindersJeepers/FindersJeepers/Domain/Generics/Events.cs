public record TripStopsClearedEvent(int routeId) : IDomainEvent
{
    public DateTime OccuredAt { get; } = DateTime.UtcNow;
}
