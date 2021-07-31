namespace MusicVideos
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Filter class to hold data related to the playlist filter.
    /// </summary>
    public class Filter
    {
        private readonly Collection<Genre> genres = new Collection<Genre>();

        /// <summary>
        /// Gets or sets the minimum rating for the filter.
        /// </summary>
        public int RatingMinimum { get; set; } = 1;

        /// <summary>
        /// Gets or sets the maximum rating for the filter.
        /// </summary>
        public int RatingMaximum { get; set; } = 100;

        /// <summary>
        /// Gets or sets the minimum date time for the filter.
        /// </summary>
        public DateTime DateTimeMinimum { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Gets or sets the maximum date time for the filter.
        /// </summary>
        public DateTime DateTimeMaximum { get; set; } = DateTime.MaxValue;

        /// <summary>
        /// Gets or sets the minimum year for the released date.
        /// </summary>
        public int ReleasedMinimum { get; set; } = 1900;

        /// <summary>
        /// Gets or sets the maximum year for the released date.
        /// </summary>
        public int ReleasedMaximum { get; set; } = 2100;

        /// <summary>
        /// Gets the collection of Genre's for the filter.
        /// </summary>
        public Collection<Genre> Genres
        {
            get { return genres; }
        }
    }
}