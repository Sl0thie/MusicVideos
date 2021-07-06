namespace MusicVideosRemote.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using SQLite;
    using MusicVideosRemote.Models;
    using System.Diagnostics;
    using System;
    using Microsoft.AspNetCore.SignalR.Client;
    using Newtonsoft.Json;

    class LocalDatabase
    {
        private static SQLiteAsyncConnection Database;
        private HubConnection dataHub;

        private string hubId = string.Empty;

        private List<Tuple<string, Exception>> Errors = new List<Tuple<string, Exception>>();
        private List<Tuple<string, string>> Messages = new List<Tuple<string, string>>();


        public static readonly AsyncLazy<LocalDatabase> Instance = new AsyncLazy<LocalDatabase>(async () =>
        {
            var instance = new LocalDatabase();
            CreateTableResult result = await Database.CreateTableAsync<Video>();
            return instance;
        });

        public LocalDatabase()
        {
            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            IinitializeSignalR();
        }

        #region SignalR

        private async void IinitializeSignalR()
        {
            Debug.WriteLine("LocalDatabase.IinitializeSignalR");

            try
            {
                dataHub = new HubConnectionBuilder()
                    .WithUrl("http://192.168.0.6:888/messageHub")
                    .Build();

                dataHub.On<string>("SetRegistration", (id) =>
                {
                    hubId = id;
                    Debug.WriteLine("Hub Registration Id: " + id);
                });

                dataHub.On<string,string>("SendMessage", (id,message) =>
                {
                    Messages.Add(new Tuple<string, string>(id,message));
                });

                dataHub.On<string, string>("SendError", (id, json) =>
                {
                    Errors.Add(new Tuple<string, Exception>(id, JsonConvert.DeserializeObject<Exception>(json)));
                });

                dataHub.On<string>("SaveVideo", async (json) =>
                {
                    Video newVideo = JsonConvert.DeserializeObject<Video>(json);
                    await SaveVideoAsync(newVideo);
                });

                await dataHub.StartAsync();

                await dataHub.InvokeAsync("RegisterRemoteAsync", "123456");

                // CallForVideos();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }
        }

        #endregion

        #region Debugging

        public async void Error(Exception ex)
        {
            await dataHub.InvokeAsync("SendErrorAsync", hubId, JsonConvert.SerializeObject(ex, Formatting.None));
        }

        #endregion

        public Task<List<Video>> GetVideosAsync()
        {
            Debug.WriteLine("LocalDatabase.GetVideosAsync");

            try
            {
                return Database.Table<Video>().ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
                return null;
            }
        }

        public void CallForVideos()
        {
            Debug.WriteLine("LocalDatabase.CallForVideos");

            try
            {
                dataHub.InvokeAsync("GetVideosAsync");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
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
    }
}
