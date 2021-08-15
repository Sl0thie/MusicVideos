namespace MusicVideosRemote.Services
{
    using System.Diagnostics;
    using MusicVideosRemote.Models;

    /// <summary>
    /// Settings class.
    /// </summary>
    public static class Settings
    {
        /// <summary>
        ///  Gets or sets the Volume.
        /// </summary>
        public static int Volume
        {
            get
            {
                return volume;
            }

            set
            {
                if (value == -1)
                {
                    return;
                }

                if (volume != value)
                {
                    volume = value;

                    if (volume > 100)
                    {
                        volume = 100;
                    }

                    if (volume < 0)
                    {
                        volume = 0;
                    }

                    Debug.WriteLine("Setting Volume " + volume);

                    _ = SignalRClient.Current.SetInVolumeAsync(volume);
                }
            }
        }

        /// <summary>
        /// Gets or sets the filter for the video playlist.
        /// </summary>
        public static Filter Filter
        {
            get
            {
                return filter;
            }

            set
            {
                filter = value;
            }
        }

        /// <summary>
        /// Gets or sets the minimum rating.
        /// </summary>
        public static int RatingMinimum
        {
            get
            {
                return filter.RatingMinimum;
            }

            set
            {
                filter.RatingMinimum = value;
                _ = SignalRClient.Current.SetInFiltersAsync();
            }
        }

        /// <summary>
        /// Gets or sets the maximum rating.
        /// </summary>
        public static int RatingMaximum
        {
            get
            {
                return filter.RatingMaximum;
            }

            set
            {
                filter.RatingMaximum = value;
                _ = SignalRClient.Current.SetInFiltersAsync();
            }
        }

        /// <summary>
        /// Gets or sets the minimum released.
        /// </summary>
        public static int ReleasedMinimum
        {
            get
            {
                return filter.ReleasedMinimum;
            }

            set
            {
                filter.ReleasedMinimum = value;
                _ = SignalRClient.Current.SetInFiltersAsync();
            }
        }

        /// <summary>
        /// Gets or sets the maximum released.
        /// </summary>
        public static int ReleasedMaximum
        {
            get
            {
                return filter.ReleasedMaximum;
            }

            set
            {
                filter.ReleasedMaximum = value;
                _ = SignalRClient.Current.SetInFiltersAsync();
            }
        }

        private static int volume = -1;
        private static Filter filter;

        /// <summary>
        /// Check if the video passes the filter.
        /// </summary>
        /// <param name="toBeChecked">The video to be checked.</param>
        /// <returns>Returns true if the video passes the filter.</returns>
        public static bool PassFilter(Video toBeChecked)
        {
            if (toBeChecked.Rating > filter.RatingMaximum)
            {
                return false;
            }

            if (toBeChecked.Rating < filter.RatingMinimum)
            {
                return false;
            }

            if (toBeChecked.ReleasedYear > filter.ReleasedMaximum)
            {
                return false;
            }

            if (toBeChecked.ReleasedYear < filter.ReleasedMinimum)
            {
                return false;
            }

            return true;
        }
    }
}