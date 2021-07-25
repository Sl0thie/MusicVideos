namespace MusicVideosRemote.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using SQLite;
    using MusicVideosRemote.Models;
    using System.Diagnostics;
    using System;

    public class DataStore
    {
        private static SQLiteAsyncConnection Database;
        // private string hubId = string.Empty;
        // private List<MessageItem> messages = new List<MessageItem>();
        // private List<ErrorItem> errors;

        //public List<MessageItem> Messages
        //{
        //    get { return messages; }
        //    set { messages = value; }
        //}

        //public List<ErrorItem> Errors
        //{
        //    get { return errors; }
        //    set { errors = value; }
        //}

        public static readonly AsyncLazy<DataStore> Instance = new AsyncLazy<DataStore>(async () =>
        {
            var instance = new DataStore();
            // _ = Database.DropTableAsync<Video>();
            CreateTableResult result = await Database.CreateTableAsync<Video>();
            return instance;
        });

        public DataStore()
        {
            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        }

        #region Video

        public Task<List<Video>> GetAllVideosAsync()
        {
            Debug.WriteLine("LocalDatabase.GetVideosAsync");

            try
            {
                ////Task<List<Video>> rv = Database.QueryAsync<Video>("SELECT * FROM [Video] WHERE [Artist] LIKE 'A%'");
                //Task<List<Video>> rv = Database.QueryAsync<Video>("SELECT * FROM [Video]");
                //List<Video> videos = rv.Result;
                //Debug.WriteLine("rv count " + rv.Result.Count);
                //return rv;
                return Database.Table<Video>().ToListAsync();
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
                ////Task<List<Video>> rv = Database.QueryAsync<Video>("SELECT * FROM [Video] WHERE [Artist] LIKE 'A%'");
                //Task<List<Video>> rv = Database.QueryAsync<Video>("SELECT * FROM [Video]");
                //List<Video> videos = rv.Result;
                //Debug.WriteLine("rv count " + rv.Result.Count);
                //return rv;
                return Database.Table<Video>().ToListAsync();
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

        #endregion
    }
}