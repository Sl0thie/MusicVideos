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
        private static SQLiteAsyncConnection Database;
        public const string DatabaseFilename = "SQLite.db3";
        public const SQLiteOpenFlags Flags =
            // open the database in read/write mode
            SQLiteOpenFlags.ReadWrite |
            // create the database if it doesn't exist
            SQLiteOpenFlags.Create |
            // enable multi-threaded database access
            SQLiteOpenFlags.SharedCache;

        private static Dictionary<int, Video> videos = new Dictionary<int, Video>();
        private static Dictionary<int, int> ratingHistogram = new Dictionary<int, int>();
        private static Settings settings = new Settings();

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
            get { return ratingHistogram; }

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

        /// <summary>
        /// Saves the Video to the database.
        /// </summary>
        /// <param name="video">The video to save.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static Task<int> SaveVideoAsync(Video video)
        {
            Debug.WriteLine("LocalDatabase.SaveVideoAsync");

            try
            {
                Task<List<Video>> rv = Database.QueryAsync<Video>("SELECT * FROM [Video] WHERE [Id] = '" + video.Id + "'");
                List<Video> videos = rv.Result;

                if (videos.Count == 1)
                {
                    return Database.UpdateAsync(video);
                }
                else
                {
                    return Database.InsertAsync(video);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Loads the video file data from a json file.
        /// </summary>
        public static void LoadVideos()
        {
            // Create database.
            Database = new SQLiteAsyncConnection(Path.Combine(FilesPath, DatabaseFilename), Flags);
            _ = Database.CreateTableAsync<Video>();

            // Create RatingHistogram.
            for (int i = 0; i < 101; i++)
            {
                ratingHistogram.Add(i, 0);
            }

            string json = File.ReadAllText(FilesPath + "\\index.json");
            videos = JsonConvert.DeserializeObject<Dictionary<int, Video>>(json);

            foreach (Video next in videos.Values)
            {
                // SaveVideoAsync(next);
                FilteredVideoIds.Add(next.Id);
                ratingHistogram[next.Rating]++;
            }

            for (int i = 0; i < 101; i++)
            {
                Debug.WriteLine(i + " " + ratingHistogram[i]);
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