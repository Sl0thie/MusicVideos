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

                return Settings.Filter;
            }

            set
            {
                Debug.WriteLine("FilterViewModel.Filter.Set");

                Settings.Filter = value;
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

                return Settings.Filter.RatingMaximum;
            }

            set
            {
                Debug.WriteLine("FilterViewModel.RatingMaximum.Set");

                if (Settings.Filter.RatingMaximum != value)
                {
                    Settings.Filter.RatingMaximum = value;
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
                Debug.WriteLine("FilterViewModel.RatingMinimum.Get " + Settings.Filter.RatingMinimum);

                return Settings.Filter.RatingMinimum;
            }

            set
            {
                Debug.WriteLine("FilterViewModel.RatingMinimum.Set " + value);

                if (Settings.Filter.RatingMinimum != value)
                {
                    Settings.Filter.RatingMinimum = value;
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
                Debug.WriteLine("FilterViewModel.ReleasedMinimum.Get " + Settings.ReleasedMinimum);

                return Settings.ReleasedMinimum;
            }

            set
            {
                Debug.WriteLine("FilterViewModel.ReleasedMinimum.Set " + value);

                if (Settings.ReleasedMinimum != value)
                {
                    Settings.ReleasedMinimum = value;
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
                Debug.WriteLine("FilterViewModel.ReleasedMaximum.Get " + Settings.ReleasedMaximum);

                return Settings.ReleasedMaximum;
            }

            set
            {
                Debug.WriteLine("FilterViewModel.ReleasedMaximum.Set " + value);

                if (Settings.ReleasedMaximum != value)
                {
                    Settings.ReleasedMaximum = value;
                    OnPropertyChanged("ReleasedMaximum");
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterViewModel"/> class.
        /// </summary>
        public FilterViewModel()
        {
            Debug.WriteLine("FilterViewModel.FilterViewModel");
        }
    }
}