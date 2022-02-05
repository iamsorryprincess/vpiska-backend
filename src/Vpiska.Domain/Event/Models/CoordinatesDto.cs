namespace Vpiska.Domain.Event.Models
{
    public sealed class CoordinatesDto
    {
        public double? X { get; set; }

        public double? Y { get; set; }

        public Coordinates ToModel() => new Coordinates()
        {
            X = X.Value,
            Y = Y.Value
        };
    }
}