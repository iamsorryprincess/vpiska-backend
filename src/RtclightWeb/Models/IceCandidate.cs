using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RtclightWeb.Models
{
    public sealed class IceCandidate
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [BsonElement("candidate")]
        public string Candidate { get; set; }

        [BsonElement("sdpMLineIndex")]
        public int SdpMLineIndex { get; set; }

        [BsonElement("sdpMid")]
        public string SdpMid { get; set; }
    }
}