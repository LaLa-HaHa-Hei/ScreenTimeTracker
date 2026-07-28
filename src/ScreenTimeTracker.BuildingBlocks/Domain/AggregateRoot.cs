namespace ScreenTimeTracker.BuildingBlocks.Domain;

public abstract class AggregateRoot : Entity
{
    [Obsolete("a", true)]
    public AggregateRoot() { }

    public AggregateRoot(Guid id)
        : base(id) { }

    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void RemoveDomainEvent(IDomainEvent domainEvent) => _domainEvents.Remove(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
