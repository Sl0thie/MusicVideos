namespace MusicVideosRemote.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using MusicVideosRemote.Models;
    using MusicVideosRemote.ViewModels;
    using SQLite;

    /// <summary>
    /// DataStore class manages database access.
    /// </summary>
    public class DataStore
    {
        /// <summary>
        /// Flags used by Sqlite.
        /// </summary>
        private const SQLiteOpenFlags Flags =
            SQLiteOpenFlags.ReadWrite |
            SQLiteOpenFlags.Create |
            SQLiteOpenFlags.SharedCache;

        private static SQLiteAsyncConnection database;

        /// <summary>
        /// Singleton access.
        /// </summary>
        public static readonly AsyncLazy<DataStore> Instance = new AsyncLazy<DataStore>(async () =>
        {
            Debug.WriteLine("DataStore.Instance");

            var instance = new DataStore();

            // _ = database.DropTableAsync<Video>(); // Uncomment to drop the table. (for testing)
            CreateTableResult result = await database.CreateTableAsync<Video>();
            return instance;
        });

        /// <summary>
        /// Initializes a new instance of the <see cref="DataStore"/> class.
        /// </summary>
        public DataStore()
        {
            Debug.WriteLine("DataStore.DataStore");

            database = new SQLiteAsyncConnection(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SQLite.db3"), Flags);
        }

        /// <summary>
        /// Gets all videos from the database.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<List<Video>> GetAllVideosAsync()
        {
            Debug.WriteLine("DataStore.GetVideosAsync");

            try
            {
                string sql = "SELECT * FROM Video ";
                sql += "ORDER BY SearchArtist, Title;";

                Debug.WriteLine($"SQL {sql}");

                Task<List<Video>> videos = database.QueryAsync<Video>(sql);
                return videos;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Get a filtered list of videos.
        /// </summary>
        /// <param name="filter">The filter to apply to the list.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<List<Video>> GetFilteredVideosAsync(Filter filter)
        {
            Debug.WriteLine("DataStore.GetFilteredVideosAsync");

            try
            {
                string sql = "SELECT * FROM Video WHERE ";
                sql += $"(Rating BETWEEN {filter.RatingMinimum} AND {filter.RatingMaximum}) ";
                sql += $"AND (ReleasedYear BETWEEN {filter.ReleasedMinimum} AND {filter.ReleasedMaximum}) ";
                sql += "ORDER BY SearchArtist, Title;";

                Debug.WriteLine($"SQL {sql}");

                Task<List<Video>> videos = database.QueryAsync<Video>(sql);
                return videos;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Get the top 100 videos.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<List<Video>> GetTop100VideosAsync()
        {
            Debug.WriteLine("DataStore.GetFilteredVideosAsync");

            try
            {
                string sql = "SELECT * FROM Video ";
                sql += "ORDER BY Rating DESC, QueuedCount DESC, PlayCount DESC LIMIT 100;";

                Debug.WriteLine($"SQL {sql}");

                Task<List<Video>> videos = database.QueryAsync<Video>(sql);
                return videos;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Searches for videos based on the search term.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<List<Video>> GetVideosFromTermAsync(string term)
        {
            Debug.WriteLine("DataStore.GetFilteredVideosAsync");

            try
            {
                string sql = "SELECT * FROM Video ";
                sql += $"WHERE (Artist LIKE '%{term}%') ";
                sql += $"OR (Title LIKE '%{term}%') ";
                sql += "ORDER BY SearchArtist, Title;";

                Debug.WriteLine($"SQL {sql}");

                Task<List<Video>> videos = database.QueryAsync<Video>(sql);
                return videos;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Saves a video object to the database.
        /// If the video object already exists then it is updated instead.
        /// </summary>
        /// <param name="video">The video to save.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<int> SaveVideoAsync(Video video)
        {
            Debug.WriteLine("DataStore.SaveVideoAsync");

            try
            {
                Task<List<Video>> rv = database.QueryAsync<Video>("SELECT * FROM [Video] WHERE [Id] = '" + video.Id + "'");
                List<Video> videos = rv.Result;

                // Update database.
                if (videos.Count == 1)
                {
                    return database.UpdateAsync(video);
                }
                else
                {
                    return database.InsertAsync(video);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Returns the total number of videos in the database.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<int> TotalVideosAsync()
        {
            Debug.WriteLine("DataStore.TotalVideosAsync");

            try
            {
                int rv = await database.ExecuteScalarAsync<int>("SELECT COUNT(Id) from Video;");
                return rv;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
                return 0;
            }
        }
    }
}