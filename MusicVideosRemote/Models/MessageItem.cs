namespace MusicVideosRemote.Models
{
    using System;

    public class MessageItem
    {
        public string HubId { get; set; }

        public string Message { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
