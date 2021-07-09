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
        private int volume;
        private int filterRating;
        private Filter filter;

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

        /// <summary>
        /// Gets or sets the filter rating.
        /// </summary>
        [Obsolete("Moving to Filter class")]
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
        /// Gets or sets the filter for the video playlist.
        /// </summary>
        public Filter Filter
        {
            get { return filter; }
            set { filter = value; }
        }

        public Settings()
        {

        }
    }
}