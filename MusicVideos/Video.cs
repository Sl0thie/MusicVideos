namespace MusicVideos
{
    using System;
    using System.Collections.ObjectModel;
    using Newtonsoft.Json;
    using SQLite;

    /// <summary>
    /// Video object to store properties related to the video file.
    /// </summary>
    public class Video
    {
        private readonly Collection<Genre> genres = new Collection<Genre>();

        /// <summary>
        /// Gets or sets the Index.
        /// </summary>
        [PrimaryKey]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the song's artist name.
        /// </summary>
        public string Artist { get; set; }

        /// <summary>
        /// Gets or sets the song's artist search name.
        /// Artist's name with prefixes such as 'the' removed.
        /// </summary>
        public string SearchArtist { get; set; }

        /// <summary>
        /// Gets or sets the song's title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the song's album.
        /// </summary>
        public string Album { get; set; }

        /// <summary>
        /// Gets or sets the UNC path to the file. (to be obsolete soon).
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets the genres that this song falls into.
        /// </summary>
        public Collection<Genre> Genres
        {
            get { return genres; }
        }

        /// <summary>
        /// Gets or sets the file extension.
        /// </summary>
        public string Extension { get; set; }

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
        /// Gets or sets the total number or times the video has been queued.
        /// </summary>
        public int QueuedCount { get; set; }

        /// <summary>
        /// Gets or sets the total time the video has been played.
        /// </summary>
        public double PlayTime { get; set; }

        /// <summary>
        /// Gets or sets the rating of the video.
        /// </summary>
        public int Rating { get; set; }

        /// <summary>
        /// Gets or sets the last time the video was played.
        /// </summary>
        public DateTime LastPlayed { get; set; }

        /// <summary>
        /// Gets or sets the last time the video was queued.
        /// </summary>
        public DateTime LastQueued { get; set; }

        /// <summary>
        /// Gets or sets when the song was released.
        /// </summary>
        public DateTime Released { get; set; }

        /// <summary>
        /// Gets or sets when the song was added to the collection.
        /// </summary>
        public DateTime Added { get; set; }

        /// <summary>
        /// Gets or sets the number of errors.
        /// </summary>
        public int Errors { get; set; }

        /// <summary>
        /// Gets or sets the physical path of the video.
        /// </summary>
        public string PhysicalPath { get; set; }

        /// <summary>
        /// Gets or sets the virtual path of the video.
        /// </summary>
        public string VirtualPath { get; set; }

        /// <summary>
        /// Gets or sets when the year song was released.
        /// </summary>
        public int ReleasedYear { get; set; }

        /// <summary>
        /// Gets or sets the checksum.
        /// </summary>
        [JsonIgnore]
        public int Checksum
        {
            get { return checksum; }
            set { checksum = value; }
        }

        private int checksum;
    }
}