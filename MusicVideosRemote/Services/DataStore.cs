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
    using System.Collections.ObjectModel;

    class DataStore
    {
        private static SQLiteAsyncConnection Database;
        private HubConnection dataHub;
        private string hubId = string.Empty;
        private List<MessageItem> messages = new List<MessageItem>();
        private List<ErrorItem> errors;
        
        public List<MessageItem> Messages
        {
            get { return messages; }
            set { messages = value; }
        }

        public List<ErrorItem> Errors
        {
            get { return errors; }
            set { errors = value; }
        }

        public static readonly AsyncLazy<DataStore> Instance = new AsyncLazy<DataStore>(async () =>
        {
            var instance = new DataStore();
            CreateTableResult result = await Database.CreateTableAsync<Video>();
            return instance;
        });

        public DataStore()
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

                dataHub.On<string,string>("SendMessage", (id, message) =>
                {
                    Debug.WriteLine("SendMessage: " + id + " " + message);
                    MessageItem nextMessage = new MessageItem
                    {
                        HubId = id,
                        Message = message,
                        Timestamp = DateTime.Now
                    };
                    Messages.Add(nextMessage);
                });

                dataHub.On<string, string>("SendError", (id, json) =>
                {
                    ErrorItem nextError = new ErrorItem
                    {
                        HubId = id,
                        Ex = JsonConvert.DeserializeObject<Exception>(json),
                        Timestamp = DateTime.Now
                    };

                    Debug.WriteLine(id + " " + json);

                    Errors.Add(nextError);
                });

                dataHub.On<string>("SaveVideo", async (json) =>
                {
                    //Debug.WriteLine("SaveVideo: " + json);
                    Video newVideo = JsonConvert.DeserializeObject<Video>(json);
                    await SaveVideoAsync(newVideo);
                });

                await dataHub.StartAsync();
                await dataHub.InvokeAsync("RegisterRemoteAsync", "123456");
                //await dataHub.InvokeAsync("SendMessageAsync", hubId, "Test Message");
                //await dataHub.InvokeAsync("SendMessageAsync", hubId, "CallForVideos");
                await dataHub.InvokeAsync("GetVideosAsync");
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
    }
}
