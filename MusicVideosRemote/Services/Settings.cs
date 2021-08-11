namespace MusicVideosRemote.Services
{
    using System;
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
                Debug.WriteLine("Settings.Volume.Get");

                return volume;
            }

            set
            {
                Debug.WriteLine("Settings.Volume.Set");

                if (value == -1)
                {
                    Debug.WriteLine("Settings.Volume = -1 not setting volume.");
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

                    //_ = SignalRClient.Current.SetInSettingsAsync(volume, filter);

                    Debug.WriteLine("Settings.Volume = " + volume);
                }
                else
                {
                    Debug.WriteLine("Settings.Volume = same = " + volume);
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
                lastFilter = filter;
                filter = value;
            }
        }

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
        private static Filter lastFilter;
        private static Filter filter;

        /// <summary>
        /// Checks if the Filters are equal.
        /// </summary>
        /// <param name="first">The first filter to check.</param>
        /// <param name="last">The second filter to check.</param>
        /// <returns>Returns true id the filters are equal.</returns>
        private static bool IsFilterEqual(Filter first, Filter last)
        {
            Debug.WriteLine("Settings.IsFilterEqual");

            bool diff = false;

            if (last is null)
            {
                Debug.WriteLine("Last is null");
                return false;
            }

            Debug.WriteLine($"RatingMaximum: first {first.RatingMaximum} second {last.RatingMaximum}");
            Debug.WriteLine($"RatingMinimum: first {first.RatingMinimum} second {last.RatingMinimum}");

            try
            {
                if (first.RatingMaximum != last.RatingMaximum)
                {
                    diff = true;
                    Debug.WriteLine($"Settings.IsFilterEqual.RatingMaximum: first {first.RatingMaximum} second {last.RatingMaximum}");
                }

                if (first.RatingMinimum != last.RatingMinimum)
                {
                    diff = true;
                    Debug.WriteLine($"Settings.IsFilterEqual.RatingMinimum: first {first.RatingMinimum} second {last.RatingMinimum}");
                }

                if (first.ReleasedMaximum != last.ReleasedMaximum)
                {
                    diff = true;
                    Debug.WriteLine($"Settings.IsFilterEqual.ReleasedMaximum: first {first.ReleasedMaximum} second {last.ReleasedMaximum}");
                }

                if (first.ReleasedMinimum != last.ReleasedMinimum)
                {
                    diff = true;
                    Debug.WriteLine($"Settings.IsFilterEqual.ReleasedMinimum: first {first.ReleasedMinimum} second {last.ReleasedMinimum}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            if (diff)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if the video passes the filter.
        /// </summary>
        /// <param name="toBeChecked">The video to be checked.</param>
        /// <returns>Returns true if the video passes the filter.</returns>
        public static bool PassFilter(Video toBeChecked)
        {
            Debug.WriteLine("Settings.PassFilter");

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