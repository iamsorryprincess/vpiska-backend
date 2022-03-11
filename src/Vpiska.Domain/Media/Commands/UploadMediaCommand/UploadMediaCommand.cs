namespace Vpiska.Domain.Media.Commands.UploadMediaCommand
{
    public sealed class UploadMediaCommand
    {
        public string Name { get; set; }

        public string ContentType { get; set; }

        public byte[] Body { get; set; }

        public Media ToModel(string id, string extension) => new()
        {
            Id = id,
            Name = Name,
            ContentType = ContentType,
            Extension = extension,
            Size = Body.Length
        };
    }
}