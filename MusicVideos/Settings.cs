namespace MusicVideos
{
    using System;
    using LogCore3;

    /// <summary>
    /// Settings class to hold settings for MusicVideo.
    /// </summary>
    public class Settings
    {
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

                if (filter != null)
                {
                    if (IsFilterEqual(filter, lastFilter))
                    {
                        Log.Info($"Filter Set: Equal so not calling property change.");
                    }
                    else
                    {
                        Log.Info($"Filter Set: Filter changed.");

                        DS.Videos.FilterVideos();
                        DS.SaveSettings();
                        _ = DS.Comms.SetInSettingsAsync(DS.Settings.Filter, DS.Settings.Volume);
                    }
                }
                else
                {
                    Log.Info($"Filter Set: Filter is null.");
                }

                // filter = value;
                // DS.Videos.FilterVideos();
                // lastFilter = filter;
                //filter = value;

                //if (filter != null)
                //{
                //    if (IsFilterEqual(filter, lastFilter))
                //    {
                //        Log.Info($"Filter Set: Equal so not calling property change.");
                //    }
                //    else
                //    {
                //        Log.Info($"Filter Set: Filter changed.");

                //        // OnPropertyChanged("Filter");
                //        // OnPropertyChanged("RatingMinimum");
                //        // FilterUpdated();

                //        // _ = SignalRClient.Current.SetInSettingsAsync(volume, filter);
                //        DS.Videos.FilterVideos();
                //        DS.SaveSettings();
                //    }
                //}
                //else
                //{
                //    Log.Info($"Filter Set: Filter is null.");
                //}
            }
        }

        private int volume = 50;
        private int filterRating;
        private Filter filter = new Filter();
        private Filter lastFilter;

        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        public Settings()
        {
        }

        /// <summary>
        /// Checks if the Filters are equal.
        /// </summary>
        /// <param name="first">The first filter to check.</param>
        /// <param name="last">The second filter to check.</param>
        /// <returns>Returns true id the filters are equal.</returns>
        public bool IsFilterEqual(Filter first, Filter last)
        {
            Log.Info("Settings.IsFilterEqual");

            bool diff = false;

            if (last is null)
            {
                return false;
            }

            if (first.RatingMaximum != last.RatingMaximum)
            {
                diff = true;
                Log.Info($"Settings.IsFilterEqual.RatingMaximum: first {first.RatingMaximum} second {last.RatingMaximum}");
            }

            if (first.RatingMinimum != last.RatingMinimum)
            {
                diff = true;
                Log.Info($"Settings.IsFilterEqual.RatingMinimum: first {first.RatingMinimum} second {last.RatingMinimum}");
            }

            if (first.ReleasedMaximum != last.ReleasedMaximum)
            {
                diff = true;
                Log.Info($"Settings.IsFilterEqual.ReleasedMaximum: first {first.ReleasedMaximum} second {last.ReleasedMaximum}");
            }

            if (first.ReleasedMinimum != last.ReleasedMinimum)
            {
                diff = true;
                Log.Info($"Settings.IsFilterEqual.ReleasedMinimum: first {first.ReleasedMinimum} second {last.ReleasedMinimum}");
            }

            if (diff)
            {
                return false;
            }

            return true;
        }
    }
}