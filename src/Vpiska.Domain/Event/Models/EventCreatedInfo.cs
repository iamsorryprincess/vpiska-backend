namespace Vpiska.Domain.Event.Models
{
    public sealed class EventCreatedInfo
    {
        public string EventId { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public int UsersCount { get; set; }

        public Coordinates Coordinates { get; set; }
    }
}