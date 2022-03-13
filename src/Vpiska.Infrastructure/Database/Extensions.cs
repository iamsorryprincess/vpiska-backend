using System;
using SharpCompress.Common;

namespace Vpiska.Infrastructure.Database
{
    public static class Extensions
    {
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
                _ => throw new InvalidFormatException($"unknown bytes dimension - {count}")
            };

            return $"{value} {dimension}";
        }
    }
}