namespace MusicVideosRemote.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using SQLite;
    using MusicVideosRemote.Models;
    using MusicVideosRemote.ViewModels;
    using System.Diagnostics;
    using System;
    using Microsoft.AspNetCore.SignalR.Client;
    using Newtonsoft.Json;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using MusicVideosRemote.Views;

    public class DataStore
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
            // _ = Database.DropTableAsync<Video>();
            CreateTableResult result = await Database.CreateTableAsync<Video>();
            return instance;
        });

        public DataStore()
        {
            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            //IinitializeSignalR();
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


        //#region SignalR

        //private async void IinitializeSignalR()
        //{
        //    Debug.WriteLine("LocalDatabase.IinitializeSignalR");

        //    try
        //    {
        //        dataHub = new HubConnectionBuilder()
        //            .WithUrl("http://192.168.0.6:888/videoHub")
        //            .Build();

        //        dataHub.On<string>("SetRegistration", (id) =>
        //        {
        //            hubId = id;
        //            Debug.WriteLine("Hub Registration Id: " + id);
        //        });

        //        dataHub.On<string,string>("SendMessage", (id, message) =>
        //        {
        //            Debug.WriteLine("SendMessage: " + id + " " + message);
        //            MessageItem nextMessage = new MessageItem
        //            {
        //                HubId = id,
        //                Message = message,
        //                Timestamp = DateTime.Now
        //            };
        //            Messages.Add(nextMessage);
        //        });

        //        dataHub.On<string, string>("SendError", (id, json) =>
        //        {
        //            ErrorItem nextError = new ErrorItem
        //            {
        //                HubId = id,
        //                Ex = JsonConvert.DeserializeObject<Exception>(json),
        //                Timestamp = DateTime.Now
        //            };

        //            Debug.WriteLine(id + " " + json);

        //            Errors.Add(nextError);
        //        });

        //        dataHub.On<string, string>("PlayVideo", (json, time) =>
        //        {
        //            Video newVideo = JsonConvert.DeserializeObject<Video>(json);
        //            NowplayingModel.Current.CurrentVideo = newVideo;
        //            Debug.WriteLine("PlayVideo: " + newVideo.Artist);
        //        });

        //        dataHub.On<string>("SaveVideo", async (json) =>
        //        {
        //            Video newVideo = JsonConvert.DeserializeObject<Video>(json);
        //            await SaveVideoAsync(newVideo);
        //        });

        //        dataHub.On<string>("SaveFilter", (json) =>
        //        {
        //            Filter newFilter = JsonConvert.DeserializeObject<Filter>(json);
        //            FilterModel.Current.Filter = newFilter;
        //        });

        //        await dataHub.StartAsync();
        //        await dataHub.InvokeAsync("RegisterRemoteAsync", "123456");
        //        //await dataHub.InvokeAsync("GetVideosAsync", hubId);
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine("Error: " + ex.Message);
        //    }
        //}

        //#endregion

        //#region Debugging

        //public async void ErrorAsync(Exception ex)
        //{
        //    await dataHub.InvokeAsync("SendErrorAsync", hubId, JsonConvert.SerializeObject(ex, Formatting.None));
        //}

        //#endregion

        //#region Filter

        //public async void SendFilterAsync(Filter filter)
        //{
        //    await dataHub.InvokeAsync("SendFilterAsync", hubId, JsonConvert.SerializeObject(filter, Formatting.None));
        //}

        //#endregion

        //#region Video

        //public Task<List<Video>> GetAllVideosAsync()
        //{
        //    Debug.WriteLine("LocalDatabase.GetVideosAsync");

        //    try
        //    {
        //        ////Task<List<Video>> rv = Database.QueryAsync<Video>("SELECT * FROM [Video] WHERE [Artist] LIKE 'A%'");
        //        //Task<List<Video>> rv = Database.QueryAsync<Video>("SELECT * FROM [Video]");
        //        //List<Video> videos = rv.Result;
        //        //Debug.WriteLine("rv count " + rv.Result.Count);
        //        //return rv;
        //        return Database.Table<Video>().ToListAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine("Error: " + ex.Message);
        //        return null;
        //    }
        //}

        //public Task<List<Video>> GetFilteredVideosAsync()
        //{
        //    Debug.WriteLine("LocalDatabase.GetVideosAsync");

        //    try
        //    {
        //        ////Task<List<Video>> rv = Database.QueryAsync<Video>("SELECT * FROM [Video] WHERE [Artist] LIKE 'A%'");
        //        //Task<List<Video>> rv = Database.QueryAsync<Video>("SELECT * FROM [Video]");
        //        //List<Video> videos = rv.Result;
        //        //Debug.WriteLine("rv count " + rv.Result.Count);
        //        //return rv;
        //        return Database.Table<Video>().ToListAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine("Error: " + ex.Message);
        //        return null;
        //    }
        //}

        //public Task<int> SaveVideoAsync(Video video)
        //{

        //    Debug.WriteLine("LocalDatabase.SaveVideoAsync");

        //    try
        //    {
        //        Task<List<Video>> rv = Database.QueryAsync<Video>("SELECT * FROM [Video] WHERE [Id] = '" + video.Id + "'");
        //        List<Video> videos = rv.Result;

        //        if (videos.Count == 1)
        //        {
        //            return Database.UpdateAsync(video);
        //        }
        //        else
        //        {
        //            return Database.InsertAsync(video);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine("Error: " + ex.Message);
        //        return null;
        //    }
        //}

        //#endregion
    }
}
