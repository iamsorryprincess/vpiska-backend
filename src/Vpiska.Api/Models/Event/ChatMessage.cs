namespace Vpiska.Api.Models.Event
{
    public sealed class ChatMessage
    {
        public string UserId { get; set; }

        public string UserName { get; set; }

        public string UserImageId { get; set; }

        public string Message { get; set; }
    }
}