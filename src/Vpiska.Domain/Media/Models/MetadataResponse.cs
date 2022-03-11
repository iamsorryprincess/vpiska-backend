namespace Vpiska.Domain.Media.Models
{
    public sealed class MetadataResponse
    {
        public string Name { get; set; }

        public string ContentType { get; set; }

        public int Size { get; set; }

        public static MetadataResponse FromModel(Media model) => new()
        {
            Name = model.Name,
            ContentType = model.ContentType,
            Size = model.Size
        };
    }
}