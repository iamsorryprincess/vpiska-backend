using System.Text.Json;

namespace Vpiska.Kafka
{
    internal static class SerializerOptions
    {
        public static JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }
}