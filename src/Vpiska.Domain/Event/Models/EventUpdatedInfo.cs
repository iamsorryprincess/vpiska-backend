namespace Vpiska.Domain.Event.Models
{
    public sealed class EventUpdatedInfo
    {
        public string EventId { get; set; }

        public string Address { get; set; }

        public int UsersCount { get; set; }

        public Coordinates Coordinates { get; set; }

        public static EventUpdatedInfo FromModel(Event model) => new EventUpdatedInfo()
        {
            EventId = model.Id,
            Address = model.Address,
            Coordinates = model.Coordinates,
            UsersCount = model.Users.Count
        };
    }
}