namespace Vpiska.Domain.Event.Commands.RemoveMediaCommand
{
    public sealed class RemoveMediaCommand
    {
        public string EventId { get; set; }

        public string OwnerId { get; set; }

        public string MediaId { get; set; }
    }
}