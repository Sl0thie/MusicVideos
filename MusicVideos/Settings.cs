namespace MusicVideos
{
    using System;

    /// <summary>
    /// Settings class to hold settings for MusicVideo.
    /// </summary>
    public class Settings
    {
        private int volume;
        private int filterRating;
        private Filter filter = new Filter();

        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        public Settings()
        {
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

        /// <summary>
        /// Gets or sets the filter rating.
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
        /// Gets or sets the filter for the video playlist.
        /// </summary>
        public Filter Filter
        {
            get
            {
                return filter;
            }

            set
            {
                filter = value;
                DS.Videos.FilterVideos();
            }
        }
    }
}