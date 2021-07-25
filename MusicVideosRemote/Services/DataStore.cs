namespace MusicVideosRemote.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using MusicVideosRemote.Models;
    using SQLite;

    /// <summary>
    /// DataStore class manages database access.
    /// </summary>
    public class DataStore
    {
        private static SQLiteAsyncConnection database;

        /// <summary>
        /// Singleton access.
        /// </summary>
        public static readonly AsyncLazy<DataStore> Instance = new AsyncLazy<DataStore>(async () =>
        {
            var instance = new DataStore();

            // _ = Database.DropTableAsync<Video>(); // Uncomment to drop the table. (for testing)
            CreateTableResult result = await database.CreateTableAsync<Video>();
            return instance;
        });

        /// <summary>
        /// Initializes a new instance of the <see cref="DataStore"/> class.
        /// </summary>
        public DataStore()
        {
            database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        }

        public Task<List<Video>> GetAllVideosAsync()
        {
            Debug.WriteLine("LocalDatabase.GetVideosAsync");

            try
            {
                return database.Table<Video>().ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
                return null;
            }
        }

        public Task<List<Video>> GetFilteredVideosAsync()
        {
            Debug.WriteLine("LocalDatabase.GetVideosAsync");

            try
            {
                return database.Table<Video>().ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
                return null;
            }
        }

        public Task<int> SaveVideoAsync(Video video)
        {
            Debug.WriteLine("LocalDatabase.SaveVideoAsync");

            try
            {
                Task<List<Video>> rv = database.QueryAsync<Video>("SELECT * FROM [Video] WHERE [Id] = '" + video.Id + "'");
                List<Video> videos = rv.Result;

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