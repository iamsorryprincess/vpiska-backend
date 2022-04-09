using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RtclightWeb.Models
{
    public sealed class Offer
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("type")]
        public string Type { get; set; }

        [BsonElement("sdp")]
        public string Sdp { get; set; }
    }
}