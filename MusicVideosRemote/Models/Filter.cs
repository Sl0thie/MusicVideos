namespace MusicVideosRemote.Models
{
    using System;
    using System.Collections.Generic;

    public class Filter
    {
        public int RatingMinimum { get; set; } = 1;

        public int RatingMaximum { get; set; } = 100;

        public DateTime DateTimeMinimum { get; set; }

        public DateTime DateTimeMaximum { get; set; }


        public List<Genre> Genres = new List<Genre>();
    }
}