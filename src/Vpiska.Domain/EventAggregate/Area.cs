namespace Vpiska.Domain.EventAggregate
{
    public sealed class Area
    {
        public string Name { get; }

        public Area(string name)
        {
            Name = name;
        }
    }
}