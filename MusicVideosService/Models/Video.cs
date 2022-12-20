namespace MusicVideosService.Models
{
    using SQLite;

    /// <summary>
    /// Video Class.
    /// </summary>
    public class Video
    {
        /// <summary>
        /// Gets or sets the Index.
        /// </summary>
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the song's artist name.
        /// </summary>
        public string Artist { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the song's artist search name.
        /// Artist's name with prefixes such as 'the' removed.
        /// </summary>
        public string SearchArtist { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the song's title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the song's album.
        /// </summary>
        public string Album { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file extension.
        /// </summary>
        public string Extension { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the duration of the video.
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Gets or sets the bit-rate of the video.
        /// </summary>
        public int VideoBitRate { get; set; }

        /// <summary>
        /// Gets or sets the width of the video.
        /// </summary>
        public int VideoWidth { get; set; }

        /// <summary>
        /// Gets or sets the height of the video.
        /// </summary>
        public int VideoHeight { get; set; }

        /// <summary>
        /// Gets or sets the frames per second of the video.
        /// </summary>
        public float VideoFPS { get; set; }

        /// <summary>
        /// Gets or sets the total number or times the video has been played.
        /// </summary>
        public int PlayCount { get; set; }

        /// <summary>
        /// Gets or sets the total number or times the video has been played.
        /// </summary>
        public int FullPlayCount { get; set; }

        /// <summary>
        /// Gets or sets the total number or times the video has been queued.
        /// </summary>
        public int QueuedCount { get; set; }

        /// <summary>
        /// Gets or sets the TotalPlayTime.
        /// </summary>
        public long TotalPlayTime { get; set; }

        /// <summary>
        /// Gets or sets the rating of the video.
        /// </summary>
        public double Rating { get; set; } = 50;

        /// <summary>
        /// Gets or sets the RandomindexLow.
        /// </summary>
        public long RandomIndexLow { get; set; }

        /// <summary>
        /// Gets or sets the RandomIndexHigh.
        /// </summary>
        public long RandomIndexHigh { get; set; }

        /// <summary>
        /// Gets or sets the calculated rating of the video.
        /// </summary>
        public double CalculatedRating { get; set; } = 0;

        /// <summary>
        /// Gets or sets the last time the video was played.
        /// </summary>
        public DateTime LastPlayed { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Gets or sets the last time the video was queued.
        /// </summary>
        public DateTime LastQueued { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Gets or sets when the song was released.
        /// </summary>
        public DateTime Released { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Gets or sets when the song was added to the collection.
        /// </summary>
        public DateTime Added { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the number of errors.
        /// </summary>
        public int Errors { get; set; }

        /// <summary>
        /// Gets or sets the physical path of the video.
        /// </summary>
        public string PhysicalPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the virtual path of the video.
        /// </summary>
        public string VirtualPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets when the year song was released.
        /// </summary>
        public int ReleasedYear { get; set; }
    }
}
