public abstract class AggregateRoot
{
    private List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;
    protected void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}
