namespace MusicVideosRemote.ViewModels
{
    using System.Diagnostics;
    using MusicVideosRemote.Models;
    using MusicVideosRemote.Services;

    /// <summary>
    /// FilterViewModel class.
    /// </summary>
    public class FilterViewModel : BaseViewModel
    {
        /// <summary>
        /// Gets or sets the current FilterViewModel.
        /// </summary>
        internal static FilterViewModel Current
        {
            get
            {
                Debug.WriteLine("FilterViewModel.Current.Get");

                if (current is null)
                {
                    Debug.WriteLine($"Current Get: Current is null.");

                    current = new FilterViewModel();
                }

                return current;
            }

            set
            {
                Debug.WriteLine("FilterViewModel.Current.Set");

                current = value;
            }
        }

        /// <summary>
        /// Gets or sets Filter.
        /// </summary>
        public Filter Filter
        {
            get
            {
                Debug.WriteLine("FilterViewModel.Filter.Get");

                // if (filter is null)
                // {
                //    Debug.WriteLine($"Filter Get: Filter is null.");
                //    _ = SignalRClient.Current.GetFilterAsync();
                // }
                // return filter;
                return Settings.Current.Filter;
            }

            set
            {
                Debug.WriteLine("FilterViewModel.Filter.Set");

                // lastFilter = filter;
                // filter = value;
                // if (filter != null)
                // {
                //    if (IsFilterEqual(filter, lastFilter))
                //    {
                //        Debug.WriteLine($"Filter Set: Equal so not calling property change.");
                //    }
                //    else
                //    {
                //        Debug.WriteLine($"Filter Set: Filter changed.");
                //        OnPropertyChanged("Filter");
                //        OnPropertyChanged("RatingMinimum");
                //        FilterUpdated();
                //    }
                // }
                // else
                // {
                //    Debug.WriteLine($"Filter Set: Filter is null.");
                // }
                lastFilter = Settings.Current.Filter;

                //Settings.Current.Filter = value;
                //if (Settings.Current.Filter != null)
                //{
                //    if (IsFilterEqual(Settings.Current.Filter, lastFilter))
                //    {
                //        Debug.WriteLine($"Filter Set: Equal so not calling property change.");
                //    }
                //    else
                //    {
                //        Debug.WriteLine($"Filter Set: Filter changed.");

                //        OnPropertyChanged("Filter");
                //        OnPropertyChanged("RatingMinimum");
                //        FilterUpdated();
                //    }
                //}
                //else
                //{
                //    Debug.WriteLine($"Filter Set: Filter is null.");
                //}
            }
        }

        /// <summary>
        /// Gets or sets the RatingMaximum.
        /// </summary>
        public int RatingMaximum
        {
            get
            {
                Debug.WriteLine("FilterViewModel.RatingMaximum.Get");

                return Settings.Current.Filter.RatingMaximum;
            }

            set
            {
                Debug.WriteLine("FilterViewModel.RatingMaximum.Set");

                if (Settings.Current.Filter.RatingMaximum != value)
                {
                    Settings.Current.Filter.RatingMaximum = value;
                    OnPropertyChanged("RatingMaximum");
                    FilterUpdated();
                }
            }
        }

        /// <summary>
        /// Gets or sets the RatingMinimum.
        /// </summary>
        public int RatingMinimum
        {
            get
            {
                Debug.WriteLine("FilterViewModel.RatingMinimum.Get " + Settings.Current.Filter.RatingMinimum);

                return Settings.Current.Filter.RatingMinimum;
            }

            set
            {
                Debug.WriteLine("FilterViewModel.RatingMinimum.Set " + value);

                if (Settings.Current.Filter.RatingMinimum != value)
                {
                    Settings.Current.Filter.RatingMinimum = value;
                    OnPropertyChanged("RatingMinimum");
                    FilterUpdated();
                }
            }
        }

        /// <summary>
        /// Gets or sets the ReleasedMinimum.
        /// </summary>
        public int ReleasedMinimum
        {
            get
            {
                Debug.WriteLine("FilterViewModel.ReleasedMinimum.Get " + Settings.Current.Filter.ReleasedMinimum);

                return Settings.Current.Filter.ReleasedMinimum;
            }

            set
            {
                Debug.WriteLine("FilterViewModel.ReleasedMinimum.Set " + value);

                if (Settings.Current.Filter.ReleasedMinimum != value)
                {
                    Settings.Current.Filter.ReleasedMinimum = value;
                    OnPropertyChanged("ReleasedMinimum");
                    FilterUpdated();
                }
            }
        }

        /// <summary>
        /// Gets or sets the ReleasedMaximum.
        /// </summary>
        public int ReleasedMaximum
        {
            get
            {
                Debug.WriteLine("FilterViewModel.ReleasedMaximum.Get " + Settings.Current.Filter.ReleasedMaximum);

                return Settings.Current.Filter.ReleasedMaximum;
            }

            set
            {
                Debug.WriteLine("FilterViewModel.ReleasedMaximum.Set " + value);

                if (Settings.Current.Filter.ReleasedMaximum != value)
                {
                    Settings.Current.Filter.ReleasedMaximum = value;
                    OnPropertyChanged("ReleasedMaximum");
                    FilterUpdated();
                }
            }
        }

        private static FilterViewModel current;
        private Filter lastFilter;

        // private Filter filter;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterViewModel"/> class.
        /// </summary>
        public FilterViewModel()
        {
            Debug.WriteLine("FilterViewModel.FilterViewModel");

            Current = this;
            // _ = SignalRClient.Current.GetFilterAsync();
        }

        /// <summary>
        /// Updates external objects.
        /// </summary>
        private void FilterUpdated()
        {
            VideosFilteredViewModel.Current.UpdateFilter();

            // _ = SignalRClient.Current.SendFilterAsync(Settings.Current.Filter);
            _ = SignalRClient.Current.SetInSettingsAsync(Settings.Current.Volume, Settings.Current.Filter);
        }
    }
}