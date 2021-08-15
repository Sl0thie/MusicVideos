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
                Log.Info("Settings.Volume.Get");
                return volume;
            }

            set
            {
                Log.Info("Settings.Volume.Set");
                if (volume != value)
                {
                    volume = value;
                    DS.SaveSettings();

                    try
                    {
                        // Send the settings objects to the clients.
                        _ = DS.Comms.SetOutVolumeAsync(DS.Settings.Volume);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }
                }
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
                Log.Info("Settings.Filter.Get");
                return filter;
            }

            set
            {
                Log.Info("Settings.Filter.Set");
                filter = value;

                try
                {
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
                            lastFilter = filter;
                            _ = DS.Comms.SetOutFilterAsync(DS.Settings.Filter);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
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

            if (first is null)
            {
                throw new ArgumentNullException("first");
            }

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