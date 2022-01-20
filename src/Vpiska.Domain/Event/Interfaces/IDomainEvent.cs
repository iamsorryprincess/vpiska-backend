namespace Vpiska.Domain.Event.Interfaces
{
    public interface IDomainEvent
    {
        public string EventId { get; }
    }
}