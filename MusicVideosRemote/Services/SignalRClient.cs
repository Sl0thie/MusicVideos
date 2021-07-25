namespace MusicVideosRemote.Services
{
    using System;
    using System.Diagnostics;
    using Microsoft.AspNetCore.SignalR.Client;
    using MusicVideosRemote.Models;
    using MusicVideosRemote.ViewModels;
    using Newtonsoft.Json;

    public class SignalRClient
    {
        private static SignalRClient current;

        internal static SignalRClient Current
        {
            get
            {
                if (current is null)
                {
                    current = new SignalRClient();
                }
                return current;
            }
            set { current = value; }
        }

        private HubConnection dataHub;
        private string hubId = string.Empty;

        public SignalRClient()
        {
            IinitializeSignalR();
        }

        private async void IinitializeSignalR()
        {
            Debug.WriteLine("SignalRClient.IinitializeSignalR");

            try
            {
                dataHub = new HubConnectionBuilder()
                    .WithUrl("http://192.168.0.6:888/videoHub")
                    .Build();

                dataHub.On<string>("SetRegistration", (id) =>
                {
                    hubId = id;
                    Debug.WriteLine("Hub Registration Id: " + id);
                });

                dataHub.On<string, string>("PlayVideo", (json, time) =>
                {
                    Video newVideo = JsonConvert.DeserializeObject<Video>(json);
                    NowplayingModel.Current.CurrentVideo = newVideo;
                    Debug.WriteLine("PlayVideo: " + newVideo.Artist);
                });

                dataHub.On<string>("SaveVideo", async (json) =>
                {
                    Debug.WriteLine($"SaveVideo:  {json}");
                    Video newVideo = JsonConvert.DeserializeObject<Video>(json);
                    DataStore data = await DataStore.Instance;
                    await data.SaveVideoAsync(newVideo);
                });

                dataHub.On<string>("SaveFilter", (json) =>
                {
                    Debug.WriteLine($"SaveFilter:  {json}");
                    Filter newFilter = JsonConvert.DeserializeObject<Filter>(json);
                    FilterViewModel.Current.Filter = newFilter;
                });

                await dataHub.StartAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }
        }

        public async void RegisterAsync()
        {
            await dataHub.InvokeAsync("RegisterRemoteAsync", "123456");

            await dataHub.InvokeAsync("GetFilterAsync", hubId);

            await dataHub.InvokeAsync("GetVideosAsync", hubId);
        }

        public async void GetAllVideosAsync()
        {
            await dataHub.InvokeAsync("GetVideosAsync", hubId);
        }

        public async void GetFilterAsync()
        {
            await dataHub.InvokeAsync("GetFilterAsync", hubId);
        }

        #region Debugging

        public async void ErrorAsync(Exception ex)
        {
            await dataHub.InvokeAsync("SendErrorAsync", hubId, JsonConvert.SerializeObject(ex, Formatting.None));
        }

        #endregion

        #region Filter

        public async void SendFilterAsync(Filter filter)
        {
            await dataHub.InvokeAsync("SendFilterAsync", hubId, JsonConvert.SerializeObject(filter, Formatting.None));
        }

        #endregion

        public async void QueueVideoAsync(int id)
        {
            await dataHub.InvokeAsync("QueueVideo", hubId, id.ToString());
        }

        #region Commands

        public async void CommandNextVideo()
        {
            await dataHub.InvokeAsync("ButtonNextVideoAsync", hubId);
        }

        #endregion



    }
}