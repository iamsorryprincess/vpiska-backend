using System;
using System.IO;
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
                "video/webm" => "webm",
                _ => throw new ContentTypeNotSupportedException()
            };
        }
        
        private const int BytesDimension = 1024;

        public static string MapToSize(this int size) => MapToSize((decimal)size);

        public static string MapToSize(this long size) => MapToSize((decimal)size);

        private static string MapToSize(decimal size)
        {
            var value = size;
            var count = 1;

            while (value >= BytesDimension)
            {
                value = Math.Round(value / BytesDimension, 2);
                count++;
            }

            var dimension = count switch
            {
                1 => "B",
                2 => "KB",
                3 => "MB",
                4 => "GB",
                5 => "TB",
                6 => "PB",
                _ => throw new InvalidDataException($"unknown bytes dimension - {count}")
            };

            return $"{value} {dimension}";
        }
    }
}