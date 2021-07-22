namespace MusicVideos
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using SQLite;

    /// <summary>
    /// Model object to hold the data.
    /// </summary>
    public static class Model
    {
        /// <summary>
        /// File name for the database.
        /// </summary>
        public const string DatabaseFilename = "SQLite.db3";

        /// <summary>
        /// Flags for the database.
        /// </summary>
        public const SQLiteOpenFlags Flags =
            SQLiteOpenFlags.ReadWrite |
            SQLiteOpenFlags.Create |
            SQLiteOpenFlags.SharedCache;

        private static readonly Dictionary<int, int> RatingHistogramValue = new Dictionary<int, int>();

        /// <summary>
        /// Physical path to the directory holding the video files.
        /// </summary>
        public const string FilesPath = @"F:\Music Videos";

        /// <summary>
        /// The virtual path of the directory holding the video files.
        /// </summary>
        public const string VirtualPath = @"/Virtual/Music Videos";

        /// <summary>
        /// Gets the data related to the video files in the collection.
        /// </summary>
        public static Dictionary<int, Video> Videos
        {
            get { return videos; }
            private set { videos = value; }
        }

        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        public static Settings Settings
        {
            get { return settings; }
            set { settings = value; }
        }

        /// <summary>
        /// Gets a Histogram for spread of video ratings.
        /// </summary>
        public static Dictionary<int, int> RatingHistogram
        {
            get { return RatingHistogramValue; }

            // set { ratingHistogram = value; }
        }

        /// <summary>
        /// Gets the list of queued videos.
        /// </summary>
        public static Collection<int> QueuedVideoIds { get; } = new Collection<int>();

        /// <summary>
        /// Gets the list of previous videos.
        /// </summary>
        public static Collection<int> PreviousVideoIds { get; } = new Collection<int>();

        /// <summary>
        /// Gets the list of filtered videos.
        /// </summary>
        public static Collection<int> FilteredVideoIds { get; } = new Collection<int>();

        private static SQLiteAsyncConnection database;
        private static Dictionary<int, Video> videos = new Dictionary<int, Video>();
        private static Settings settings = new Settings();

        /// <summary>
        /// Loads the video file data from a json file.
        /// </summary>
        public static void LoadVideos()
        {
            // Create database.
            database = new SQLiteAsyncConnection(Path.Combine(FilesPath, DatabaseFilename), Flags);
            _ = database.CreateTableAsync<Video>();

            // Create RatingHistogram.
            for (int i = 0; i < 101; i++)
            {
                RatingHistogramValue.Add(i, 0);
            }

            string json = File.ReadAllText(FilesPath + "\\index.json");
            videos = JsonConvert.DeserializeObject<Dictionary<int, Video>>(json);

            foreach (Video next in videos.Values)
            {
                // SaveVideoAsync(next);
                FilteredVideoIds.Add(next.Id);
                RatingHistogramValue[next.Rating]++;
            }
        }

        /// <summary>
        /// Loads the settings from file.
        /// </summary>
        public static void LoadSettings()
        {
            if (File.Exists("settings.json"))
            {
                string json = File.ReadAllText("settings.json");
                settings = JsonConvert.DeserializeObject<Settings>(json);
            }
        }

        /// <summary>
        /// Save the settings to file.
        /// </summary>
        public static void SaveSettings()
        {
            string json = JsonConvert.SerializeObject(settings, Formatting.None);
            File.WriteAllText("settings.json", json);
        }

        /// <summary>
        /// Saves the video file data to disk.
        /// </summary>
        public static void SaveVideos()
        {
            string json = JsonConvert.SerializeObject(videos, Formatting.None);
            File.WriteAllText(FilesPath + "\\index.json", json);
        }
    }
}