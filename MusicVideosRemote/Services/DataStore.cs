namespace MusicVideosRemote.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using MusicVideosRemote.Models;
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

        /// <summary>
        /// The size of the checksum block sum.
        /// </summary>
        private const int BlockSize = 40;

        // private static SQLiteAsyncConnection database;
        private static SQLiteAsyncConnection database;

        /// <summary>
        /// Singleton access.
        /// </summary>
        internal static readonly AsyncLazy<DataStore> Instance = new AsyncLazy<DataStore>(async () =>
        {
            Debug.WriteLine("DataStore.Instance");

            DataStore instance = new DataStore();
            CreateTableResult result = await database.CreateTableAsync<Video>();
            return instance;
        });

        /// <summary>
        /// Initializes a new instance of the <see cref="DataStore"/> class.
        /// </summary>
        private DataStore()
        {
            Debug.WriteLine("DataStore.DataStore");

            database = new SQLiteAsyncConnection(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SQLite.db3"), Flags);
        }

        /// <summary>
        /// Checks the block by checksum.
        /// </summary>
        /// <param name="index">The start index of the block.</param>
        /// <param name="checksum">The checksum for the block.</param>
        public void CheckSumOfBlock(int index, int checksum)
        {
            Debug.WriteLine("DataStore.Checksum");

            int newchecksum = 0;

            try
            {
                for (int j = 0; j < BlockSize; j++)
                {
                    int id = index + j;
                    Task<List<Video>> rv = database.QueryAsync<Video>("SELECT * FROM [Video] WHERE [Id] = '" + id + "'");
                    List<Video> videos = rv.Result;
                    Video nextVideo = videos[0];

                    newchecksum += nextVideo.Checksum;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }

            if (newchecksum != checksum)
            {
                _ = SignalRClient.Current.FailedChecksumAsync(index);
            }

            Debug.WriteLine($"Checksum compare for {index} server = {checksum} client = {newchecksum} diff = {checksum - newchecksum}");
        }

        /// <summary>
        /// Gets a List of all videos from the database.
        /// </summary>
        /// <returns>A <see cref="List"/> of <see cref="Video"/> objects.</returns>
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
        /// Get a filtered List of Videos based on the filter argument.
        /// </summary>
        /// <param name="filter">The filter used to restrict the List.</param>
        /// <returns>A <see cref="List"/> of <see cref="Video"/> objects.</returns>
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
        /// Get a List top 100 Videos.
        /// </summary>
        /// <returns>A <see cref="List"/> of <see cref="Video"/> objects.</returns>
        public Task<List<Video>> GetTop100VideosAsync()
        {
            Debug.WriteLine("DataStore.GetFilteredVideosAsync");

            try
            {
                string sql = "SELECT * FROM Video ";

                // sql += "ORDER BY Rating DESC, QueuedCount DESC, PlayCount DESC LIMIT 100;";
                sql += "ORDER BY Rating DESC, QueuedCount DESC, PlayCount;";

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
        /// Gets a List of Videos that match the term argument.
        /// </summary>
        /// <param name="term">The search term.</param>
        /// <returns>A <see cref="List"/> of <see cref="Video"/> objects.</returns>
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
        /// Saves a Video object to the database.
        /// If the Video object already exists then it is updated instead of added.
        /// </summary>
        /// <param name="video">The <see cref="Video"/> object to save to the database.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<int> SaveVideoAsync(Video video)
        {
            Debug.WriteLine($"DataStore.SaveVideoAsync {video.Artist} - {video.Title}");

            // Get the current checksum of the object before saving the video.
            StringBuilder stringBuilder = new StringBuilder();
            _ = stringBuilder.Append(video.Added.ToBinary()).Append(video.Album).Append(video.Artist).Append(video.Duration).Append(video.Errors);
            _ = stringBuilder.Append(video.Extension).Append(video.Id).Append(video.LastPlayed.ToBinary()).Append(video.LastQueued.ToBinary()).Append(video.PhysicalPath);
            _ = stringBuilder.Append(video.PlayCount).Append(video.PlayTime).Append(video.QueuedCount).Append(video.Rating).Append(video.ReleasedYear);
            _ = stringBuilder.Append(video.SearchArtist).Append(video.Title).Append(video.VideoHeight).Append(video.VideoWidth).Append(video.VirtualPath);
            string str = stringBuilder.ToString();
            video.Checksum = 0;
            byte[] binary = Encoding.Unicode.GetBytes(str);

            foreach (byte b in binary)
            {
                video.Checksum += b;
            }

            // Add to database.
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
    }
}