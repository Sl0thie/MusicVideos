namespace MusicVideosRemote.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Filter class.
    /// </summary>
    public class Filter : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Interface

        /// <summary>
        /// Occurs when a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Invoke event when properties change.
        /// </summary>
        /// <param name="propertyName">The property name that changed.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
            {
                return;
            }

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        /// <summary>
        /// Gets or sets the RatingMinimum. The lowest accepted value for the video rating.
        /// </summary>
        public int RatingMinimum
        {
            get
            {
                return ratingMinimum;
            }

            set
            {
                ratingMinimum = value;
                OnPropertyChanged("RatingMinimum");
            }
        }

        /// <summary>
        /// Gets or sets the RatingMaximum. The highest accepted value for the video rating.
        /// </summary>
        public int RatingMaximum
        {
            get
            {
                return ratingMaximum;
            }

            set
            {
                ratingMaximum = value;
                OnPropertyChanged("RatingMaximum");
            }
        }

        /// <summary>
        /// Gets or sets the minimum year for the released date.
        /// </summary>
        public int ReleasedMinimum { get; set; } = 1900;

        /// <summary>
        /// Gets or sets the maximum year for the released date.
        /// </summary>
        public int ReleasedMaximum { get; set; } = 2100;

        /// <summary>
        /// Gets or sets the DateTimeMinimum. The earliest accepted date for the video.
        /// </summary>
        public DateTime DateTimeMinimum
        {
            get
            {
                return dateTimeMinimum;
            }

            set
            {
                dateTimeMinimum = value;
                OnPropertyChanged("DateTimeMinimum");
            }
        }

        /// <summary>
        /// Gets or sets the DateTimeMaximum. The latest accepted date for the video.
        /// </summary>
        public DateTime DateTimeMaximum
        {
            get
            {
                return dateTimeMaximum;
            }

            set
            {
                dateTimeMaximum = value;
                OnPropertyChanged("DateTimeMaximum");
            }
        }

        /// <summary>
        /// Gets or sets the Genres.
        /// </summary>
        public List<Genre> Genres
        {
            get
            {
                return genres;
            }

            set
            {
                genres = value;
                OnPropertyChanged("Genres");
            }
        }

        private List<Genre> genres = new List<Genre>();
        private int ratingMinimum = 1;
        private int ratingMaximum = 100;
        private DateTime dateTimeMinimum;
        private DateTime dateTimeMaximum;

        /// <summary>
        /// Initializes a new instance of the <see cref="Filter"/> class.
        /// </summary>
        public Filter()
        {
        }
    }
}