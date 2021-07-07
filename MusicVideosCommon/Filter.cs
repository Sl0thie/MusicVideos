namespace MusicVideosRemote.Models
{
    using System;
    using System.Collections.Generic;
    

    public class Filter
    {
        public int RatingMinimum { get; set; }

        public int RatingMaximum { get; set; }

        public DateTime DateTimeMinimum { get; set; }

        public DateTime DateTimeMaximum { get; set; }


        public List<Genre> Genres = new List<Genre>();
    }
}
