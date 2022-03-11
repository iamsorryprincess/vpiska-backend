using Vpiska.Domain.Media.Exceptions;

namespace Vpiska.Domain.Media
{
    public static class Extensions
    {
        public static string GetExtension(this string contentType)
        {
            return contentType switch
            {
                "image/gif" => "gif",
                "image/jpeg" => "jpeg",
                "image/pjpeg" => "jpeg",
                "image/png" => "png",
                "image/svg+xml" => "svg",
                "image/webp" => "webp",
                "video/mp4" => "mp4",
                "video/webm" => "video/webm",
                _ => throw new ContentTypeNotSupportedException()
            };
        }
    }
}