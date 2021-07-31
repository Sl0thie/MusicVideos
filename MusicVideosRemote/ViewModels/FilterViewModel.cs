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

                if (filter is null)
                {
                    Debug.WriteLine($"Filter Get: Filter is null.");
                    _ = SignalRClient.Current.GetFilterAsync();
                }

                return filter;
            }

            set
            {
                Debug.WriteLine("FilterViewModel.Filter.Set");

                lastFilter = filter;
                filter = value;
                if (filter != null)
                {
                    if (IsFilterEqual(filter, lastFilter))
                    {
                        Debug.WriteLine($"Filter Set: Equal so not calling property change.");
                    }
                    else
                    {
                        Debug.WriteLine($"Filter Set: Filter changed.");

                        OnPropertyChanged("Filter");
                        OnPropertyChanged("RatingMinimum");
                        FilterUpdated();
                    }
                }
                else
                {
                    Debug.WriteLine($"Filter Set: Filter is null.");
                }
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

                return filter.RatingMaximum;
            }

            set
            {
                Debug.WriteLine("FilterViewModel.RatingMaximum.Set");

                if (filter.RatingMaximum != value)
                {
                    filter.RatingMaximum = value;
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
                Debug.WriteLine("FilterViewModel.RatingMinimum.Get " + filter.RatingMinimum);

                return filter.RatingMinimum;
            }

            set
            {
                Debug.WriteLine("FilterViewModel.RatingMinimum.Set " + value);

                if (filter.RatingMinimum != value)
                {
                    filter.RatingMinimum = value;
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
                Debug.WriteLine("FilterViewModel.ReleasedMinimum.Get " + filter.ReleasedMinimum);

                return filter.ReleasedMinimum;
            }

            set
            {
                Debug.WriteLine("FilterViewModel.ReleasedMinimum.Set " + value);

                if (filter.ReleasedMinimum != value)
                {
                    filter.ReleasedMinimum = value;
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
                Debug.WriteLine("FilterViewModel.ReleasedMaximum.Get " + filter.ReleasedMaximum);

                return filter.ReleasedMaximum;
            }

            set
            {
                Debug.WriteLine("FilterViewModel.ReleasedMaximum.Set " + value);

                if (filter.ReleasedMaximum != value)
                {
                    filter.ReleasedMaximum = value;
                    OnPropertyChanged("ReleasedMaximum");
                    FilterUpdated();
                }
            }
        }

        private static FilterViewModel current;
        private Filter lastFilter;
        private Filter filter;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterViewModel"/> class.
        /// </summary>
        public FilterViewModel()
        {
            Debug.WriteLine("FilterViewModel.FilterViewModel");

            Current = this;
            _ = SignalRClient.Current.GetFilterAsync();
        }

        /// <summary>
        /// Updates external objects.
        /// </summary>
        private void FilterUpdated()
        {
            VideosFilteredViewModel.Current.UpdateFilter();
            _ = SignalRClient.Current.SendFilterAsync(filter);
        }

        /// <summary>
        /// Checks if the Filters are equal.
        /// </summary>
        /// <param name="first">The first filter to check.</param>
        /// <param name="second">The second filter to check.</param>
        /// <returns>Returns true id the filters are equal.</returns>
        private bool IsFilterEqual(Filter first, Filter second)
        {
            Debug.WriteLine("FilterViewModel.IsFilterEqual");

            bool diff = false;
            if (first.DateTimeMaximum != second.DateTimeMaximum)
            {
                diff = true;
                Debug.WriteLine($"IsFilterEqual.DateTimeMaximum: first {first.DateTimeMaximum} second {filter.DateTimeMaximum}");
            }

            if (first.DateTimeMinimum != second.DateTimeMinimum)
            {
                diff = true;
                Debug.WriteLine($"IsFilterEqual.DateTimeMinimum: first {first.DateTimeMinimum} second {filter.DateTimeMinimum}");
            }

            if (first.RatingMaximum != second.RatingMaximum)
            {
                diff = true;
                Debug.WriteLine($"IsFilterEqual.RatingMaximum: first {first.RatingMaximum} second {filter.RatingMaximum}");
            }

            if (first.RatingMinimum != second.RatingMinimum)
            {
                diff = true;
                Debug.WriteLine($"IsFilterEqual.RatingMinimum: first {first.RatingMinimum} second {filter.RatingMinimum}");
            }

            if (first.ReleasedMaximum != second.ReleasedMaximum)
            {
                diff = true;
                Debug.WriteLine($"IsFilterEqual.ReleasedMaximum: first {first.ReleasedMaximum} second {filter.ReleasedMaximum}");
            }

            if (first.ReleasedMinimum != second.ReleasedMinimum)
            {
                diff = true;
                Debug.WriteLine($"IsFilterEqual.ReleasedMinimum: first {first.ReleasedMinimum} second {filter.ReleasedMinimum}");
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
            Debug.WriteLine("FilterViewModel.PassFilter");

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