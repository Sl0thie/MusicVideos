namespace MusicVideos
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Settings class to hold settings for MusicVideo.
    /// </summary>
    public class Settings
    {
        private int volume;
        private int filterRating;
        private Filter filter = new Filter();

        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        public Settings()
        {
            CheckFilter();
        }

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
                Model.SaveSettings();
            }
        }

        /// <summary>
        /// Gets or sets the filter rating.
        /// </summary>
        [Obsolete("Moving to Filter class")]
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
                DS.Videos.FilterVideos();
            }
        }

        private void CheckFilter()
        {
            if (filter != null)
            {
                // Check that at least one genre is selected.
                if (filter.Genres.Count == 0)
                {
                    filter.Genres.Add(Genre.Alternative);
                    filter.Genres.Add(Genre.Blues);
                    filter.Genres.Add(Genre.Classical);
                    filter.Genres.Add(Genre.Country);
                    filter.Genres.Add(Genre.Dance);
                    filter.Genres.Add(Genre.Dubstep);
                    filter.Genres.Add(Genre.EasyListening);
                    filter.Genres.Add(Genre.Electronic);
                    filter.Genres.Add(Genre.Grunge);
                    filter.Genres.Add(Genre.HipHop);
                    filter.Genres.Add(Genre.House);
                    filter.Genres.Add(Genre.Jazz);
                    filter.Genres.Add(Genre.Metal);
                    filter.Genres.Add(Genre.Pop);
                    filter.Genres.Add(Genre.Punk);
                    filter.Genres.Add(Genre.Reggae);
                    filter.Genres.Add(Genre.RhythmAndBlues);
                    filter.Genres.Add(Genre.Rock);
                    filter.Genres.Add(Genre.Ska);
                    filter.Genres.Add(Genre.Techno);
                }

                if (filter.RatingMinimum != 50)
                {
                    filter.RatingMinimum = 50;
                }

                if (filter.RatingMaximum != 100)
                {
                    filter.RatingMaximum = 100;
                }

                filter.DateTimeMaximum = DateTime.Now.AddDays(1);
                filter.DateTimeMinimum = DateTime.Parse("1/1/1940");
            }
        }
    }
}