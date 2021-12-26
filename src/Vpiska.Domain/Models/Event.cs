using System.Collections.Generic;

namespace Vpiska.Domain.Models
{
    public sealed class Event
    {
        public string Id { get; set; }

        public string OwnerId { get; set; }

        public string Name { get; set; }
        
        public string Address { get; set; }

        public Coordinates Coordinates { get; set; } = new Coordinates();

        public List<string> MediaLinks { get; set; } = new List<string>();

        public List<UserInfo> Users { get; set; } = new List<UserInfo>();

        public List<ChatData> ChatData { get; set; } = new List<ChatData>();
    }
}