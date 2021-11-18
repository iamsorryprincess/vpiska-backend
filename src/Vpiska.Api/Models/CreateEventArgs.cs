namespace Vpiska.Api.Models
{
    public sealed class CreateEventArgs
    {
        public string Name { get; set; }

        public string Coordinates { get; set; }

        public string Address { get; set; }

        public string Area { get; set; }
    }
}