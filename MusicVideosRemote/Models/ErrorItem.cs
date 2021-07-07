namespace MusicVideosRemote.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class ErrorItem
    {
        public string HubId { get; set; }

        public Exception Ex { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
