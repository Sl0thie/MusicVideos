using System;
using System.Collections.Generic;

namespace MusicVideos
{
    /// <summary>
    /// Video object to store properties related to the video file.
    /// </summary>
    public class Video
    {
        private List<Genre> genres = new List<Genre>();

        /// <summary>
        /// Gets or sets the Index.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the song's artist name.
        /// </summary>
        public string Artist { get; set; }

        /// <summary>
        /// Gets or sets the song's title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the UNC path to the file.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the genres that this song falls into.
        /// </summary>
        public List<Genre> Genres
        {
            get { return genres; }
            set { genres = value; }
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
        /// Gets or sets the bitrate of the video.
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
        /// Gets or sets the total number or times the video has beem played.
        /// </summary>
        public int PlayCount { get; set; }

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
        /// Gets or sets when the song was released.
        /// </summary>
        public DateTime Released { get; set; }
    }
}