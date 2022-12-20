namespace MusicVideosService.Models
{

    using SQLite;

    public class Played
    {
        /// <summary>
        /// Gets or sets the Index.
        /// </summary>
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        public int VideoId { get; set; }

        public DateTime Start { get; set; }

        public DateTime Finish { get; set; }

        public int Duration { get; set; }
    }
}
