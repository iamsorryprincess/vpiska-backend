namespace Vpiska.Domain.Event.Commands.CloseEventCommand
{
    public sealed class CloseEventCommand
    {
        public string EventId { get; set; }

        public string OwnerId { get; set; }
    }
}