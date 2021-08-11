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
        /// Gets or sets Filter.
        /// </summary>
        public Filter Filter
        {
            get
            {
                Debug.WriteLine("FilterViewModel.Filter.Get");

                return Settings.Current.Filter;
            }

            set
            {
                Debug.WriteLine("FilterViewModel.Filter.Set");

                Settings.Current.Filter = value;
                OnPropertyChanged("Filter");
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
                }
            }
        }

        private static FilterViewModel current;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterViewModel"/> class.
        /// </summary>
        public FilterViewModel()
        {
            Debug.WriteLine("FilterViewModel.FilterViewModel");
        }

        /// <summary>
        /// Updates external objects.
        /// </summary>
        private void FilterUpdated()
        {
            _ = SignalRClient.Current.SetInSettingsAsync(Settings.Current.Volume, Settings.Current.Filter);
        }
    }
}