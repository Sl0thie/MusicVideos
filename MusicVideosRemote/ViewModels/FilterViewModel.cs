namespace MusicVideosRemote.ViewModels
{
    using System.Diagnostics;
    using MusicVideosRemote.Models;
    using MusicVideosRemote.Services;

    /// <summary>
    /// FilterViewModel class.
    /// </summary>
    internal class FilterViewModel : BaseViewModel
    {
        /// <summary>
        /// Gets or sets Filter.
        /// </summary>
        public Filter Filter
        {
            get
            {
                return Settings.Filter;
            }

            set
            {
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
                return Settings.Filter.RatingMaximum;
            }

            set
            {
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
                return Settings.Filter.RatingMinimum;
            }

            set
            {
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
                return Settings.ReleasedMinimum;
            }

            set
            {
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
                return Settings.ReleasedMaximum;
            }

            set
            {
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
        }
    }
}