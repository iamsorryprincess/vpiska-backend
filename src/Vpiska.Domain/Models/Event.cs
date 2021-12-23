namespace Vpiska.Domain.Models
{
    public sealed class Event
    {
        public string Id { get; set; }

        public string OwnerId { get; set; }

        public string Name { get; set; }
        
        public string Address { get; set; }

        public Coordinates Coordinates { get; set; }

        public string[] MediaLinks { get; set; }

        public UserInfo[] Users { get; set; }

        public ChatData[] ChatData { get; set; }
    }
}