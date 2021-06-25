namespace MusicVideos
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Settings class to hold settings for MusicVideo.
    /// </summary>
    public class Settings
    {
        private int filterRating;
        private int volume;

        /// <summary>
        /// Gets or sets the FilterRating, the minimum rating a video has before it is excluded from the list.
        /// </summary>
        public int FilterRating
        {
            get
            {
                return filterRating;
            }

            set
            {
                filterRating = value;
                Model.SaveSettings();
            }
        }

        /// <summary>
        ///  Gets or sets the Volume.
        /// </summary>
        public int Volume
        {
            get
            {
                return volume;
            }

            set
            {
                volume = value;
                Model.SaveSettings();
            }
        }
    }
}