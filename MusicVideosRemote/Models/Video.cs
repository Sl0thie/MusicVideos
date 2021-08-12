namespace MusicVideosRemote.Models
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using Newtonsoft.Json;
    using SQLite;

    /// <summary>
    /// Video object to store properties related to the video file.
    /// </summary>
    public class Video : INotifyPropertyChanged
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
        /// Gets or sets the id of the video.
        /// </summary>
        [PrimaryKey]
        public int Id
        {
            get
            {
                return id;
            }

            set
            {
                id = value;
                OnPropertyChanged("Id");
            }
        }

        /// <summary>
        /// Gets or sets the song's artist name.
        /// </summary>
        public string Artist
        {
            get
            {
                return artist;
            }

            set
            {
                artist = value;
                OnPropertyChanged("Artist");
            }
        }

        /// <summary>
        /// Gets or sets the song's artist search name.
        /// Artist's name with prefixes such as 'the' removed.
        /// </summary>
        public string SearchArtist
        {
            get
            {
                return searchArtist;
            }

            set
            {
                searchArtist = value;
                OnPropertyChanged("SearchArtist");
            }
        }

        /// <summary>
        /// Gets or sets the song's title.
        /// </summary
        public string Title
        {
            get
            {
                return title;
            }

            set
            {
                title = value;
                OnPropertyChanged("Title");
            }
        }

        /// <summary>
        /// Gets or sets the song's album.
        /// </summary>
        public string Album { get => album; set => album = value; }

        /// <summary>
        /// Gets or sets the UNC path to the file.
        /// </summary>
        [Obsolete("Use Physical and Virtual Path instead.")]
        public string Path { get => path; set => path = value; }

        /// <summary>
        /// Gets the genres that this song falls into.
        /// </summary>
        public Collection<Genre> Genres { get; } = new Collection<Genre>();

        /// <summary>
        /// Gets or sets the file extension.
        /// </summary>
        public string Extension { get => extension; set => extension = value; }

        /// <summary>
        /// Gets or sets the duration of the video.
        /// </summary>
        public int Duration { get => duration; set => duration = value; }

        /// <summary>
        /// Gets or sets the bit-rate of the video.
        /// </summary>
        public int VideoBitRate { get => videoBitRate; set => videoBitRate = value; }

        /// <summary>
        /// Gets or sets the width of the video.
        /// </summary>
        public int VideoWidth { get => videoWidth; set => videoWidth = value; }

        /// <summary>
        /// Gets or sets the height of the video.
        /// </summary>
        public int VideoHeight { get => videoHeight; set => videoHeight = value; }

        /// <summary>
        /// Gets or sets the frames per second of the video.
        /// </summary>
        public float VideoFPS { get => videoFPS; set => videoFPS = value; }

        /// <summary>
        /// Gets or sets the total number or times the video has been played.
        /// </summary>
        public int PlayCount { get => playCount; set => playCount = value; }

        /// <summary>
        /// Gets or sets the total number or times the video has been queued.
        /// </summary>
        public int QueuedCount { get => queuedCount; set => queuedCount = value; }

        /// <summary>
        /// Gets or sets the total time the video has been played.
        /// </summary>
        public double PlayTime { get => playTime; set => playTime = value; }

        /// <summary>
        /// Gets or sets the rating of the video.
        /// </summary>
        public int Rating { get => rating; set => rating = value; }

        /// <summary>
        /// Gets or sets the last time the video was played.
        /// </summary>
        public DateTime LastPlayed { get => lastPlayed; set => lastPlayed = value; }

        /// <summary>
        /// Gets or sets the last time the video was queued.
        /// </summary>
        public DateTime LastQueued { get => lastQueued; set => lastQueued = value; }

        /// <summary>
        /// Gets or sets when the song was released.
        /// </summary>
        [Obsolete("Use Release Year instead.")]
        public DateTime Released { get => released; set => released = value; }

        /// <summary>
        /// Gets or sets when the song was added to the collection.
        /// </summary>
        public DateTime Added { get => added; set => added = value; }

        /// <summary>
        /// Gets or sets the number of errors.
        /// </summary>
        public int Errors { get => errors; set => errors = value; }

        /// <summary>
        /// Gets or sets the physical path of the video.
        /// </summary>
        public string PhysicalPath { get => physicalPath; set => physicalPath = value; }

        /// <summary>
        /// Gets or sets the virtual path of the video.
        /// </summary>
        public string VirtualPath { get => virtualPath; set => virtualPath = value; }

        /// <summary>
        /// Gets or sets when the year song was released.
        /// </summary>
        public int ReleasedYear { get => releasedYear; set => releasedYear = value; }

        /// <summary>
        /// Gets or sets the top 100 index.
        /// </summary>
        [JsonIgnore]
        public int Top100
        {
            get { return top100; }
            set { top100 = value; }
        }

        /// <summary>
        /// Gets or sets the checksum.
        /// </summary>
        [JsonIgnore]
        public int Checksum
        {
            get { return checksum; }
            set { checksum = value; }
        }

        private int id;
        private string artist;
        private string searchArtist;
        private string title;
        private string album;
        private string path;
        private string extension;
        private int duration;
        private int videoBitRate;
        private int videoWidth;
        private int videoHeight;
        private float videoFPS;
        private int playCount;
        private int queuedCount;
        private double playTime;
        private int rating;
        private DateTime lastPlayed;
        private DateTime lastQueued;
        private DateTime released;
        private DateTime added;
        private int errors;
        private string physicalPath;
        private string virtualPath;
        private int releasedYear;
        private int top100;
        private int checksum;
    }
}