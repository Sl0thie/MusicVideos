namespace MusicVideos
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// TimelineItem class for items on the time line.
    /// </summary>
    [Obsolete("No longer in use.")]
    public class TimelineItem
    {
        /// <summary>
        /// Gets or sets the time stamp for the item.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the action type for the item.
        /// </summary>
        public string ActionType { get; set; }

        /// <summary>
        /// Gets or sets Unused.
        /// </summary>
        public Action ActionItem { get; set; }
    }
}
