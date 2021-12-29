namespace Vpiska.Domain.Base
{
    public abstract class ChatEvent : DomainEvent
    {
        public string EventId { get; set; }
    }
}