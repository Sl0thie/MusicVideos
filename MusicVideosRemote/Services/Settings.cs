namespace MusicVideosRemote.Services
{
    using System.Diagnostics;
    using MusicVideosRemote.Models;

    /// <summary>
    /// Settings class.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Gets or sets the current SignalRClient.
        /// </summary>
        internal static Settings Current
        {
            get
            {
                Debug.WriteLine("Settings.Current.Get");

                if (current is null)
                {
                    current = new Settings();
                }

                return current;
            }

            set
            {
                Debug.WriteLine("Settings.Current.Set");

                current = value;
            }
        }

        /// <summary>
        ///  Gets or sets the Volume.
        /// </summary>
        public int Volume
        {
            get
            {
                Debug.WriteLine("Settings.Volume.Get");

                return volume;
            }

            set
            {
                Debug.WriteLine("Settings.Volume.Set");

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

                    _ = SignalRClient.Current.SetInSettingsAsync(volume, filter);

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
        public Filter Filter
        {
            get
            {
                Debug.WriteLine("Settings.Filter.Get");

                if (filter is null)
                {
                    Debug.WriteLine($"Filter Get: Filter is null.");

                    _ = SignalRClient.Current.GetOutSettingsAsync();
                }

                return filter;
            }

            set
            {
                Debug.WriteLine("Settings.Filter.Set");

                lastFilter = filter;

                if (value != null)
                {
                    if (IsFilterEqual(value, lastFilter))
                    {
                        Debug.WriteLine($"Filter Set: Equal so not calling property change.");
                    }
                    else
                    {
                        Debug.WriteLine($"Filter Set: Filter changed.");

                        filter = value;
                        _ = SignalRClient.Current.SetInSettingsAsync(volume, filter);
                    }
                }
                else
                {
                    Debug.WriteLine($"Filter Set: Filter is null.");
                }
            }
        }

        private static Settings current;
        private int volume;
        private Filter lastFilter;
        private Filter filter = new Filter();

        private Settings()
        {
            Current = this;
        }

        /// <summary>
        /// Checks if the Filters are equal.
        /// </summary>
        /// <param name="first">The first filter to check.</param>
        /// <param name="last">The second filter to check.</param>
        /// <returns>Returns true id the filters are equal.</returns>
        private bool IsFilterEqual(Filter first, Filter last)
        {
            Debug.WriteLine("Settings.IsFilterEqual");

            bool diff = false;

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
        public bool PassFilter(Video toBeChecked)
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