namespace MusicVideosRemote.Services
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.SignalR.Client;
    using MusicVideosRemote.Models;
    using MusicVideosRemote.ViewModels;
    using Newtonsoft.Json;

    /// <summary>
    /// SignalRClient class manages communications to the SignalR server.
    /// </summary>
    public class SignalRClient
    {
        private static SignalRClient current;

        /// <summary>
        /// Gets or sets the current SignalRClient.
        /// </summary>
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

            set
            {
                current = value;
            }
        }

        private HubConnection dataHub;
        private string hubId = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalRClient"/> class.
        /// </summary>
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

                await RegisterAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Invokes Registration with the server.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RegisterAsync()
        {
            await dataHub.InvokeAsync("RegisterRemoteAsync", "123456");

            // await dataHub.InvokeAsync("GetVideosAsync", hubId); // Uncomment to update all videos from server.
        }

        /// <summary>
        /// Invokes Get All Videos command on server.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task GetAllVideosAsync()
        {
            await dataHub.InvokeAsync("GetVideosAsync", hubId);
        }

        /// <summary>
        /// Invokes Get Filter command on the server.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task GetFilterAsync()
        {
            await dataHub.InvokeAsync("GetFilterAsync", hubId);
        }

        #region Debugging

        /// <summary>
        /// Passes an exception to the server.
        /// </summary>
        /// <param name="ex">The exception to pass.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ErrorAsync(Exception ex)
        {
            await dataHub.InvokeAsync("SendErrorAsync", hubId, JsonConvert.SerializeObject(ex, Formatting.None));
        }

        #endregion

        #region Filter

        /// <summary>
        /// Passes a filter to the server.
        /// </summary>
        /// <param name="filter">The filter to pass.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SendFilterAsync(Filter filter)
        {
            await dataHub.InvokeAsync("SendFilterAsync", hubId, JsonConvert.SerializeObject(filter, Formatting.None));
        }

        #endregion

        /// <summary>
        /// Queues a video on the server.
        /// </summary>
        /// <param name="id">The id of the video to queue.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task QueueVideoAsync(int id)
        {
            await dataHub.InvokeAsync("QueueVideo", hubId, id.ToString());
        }

        #region Commands

        /// <summary>
        /// Invokes the Next Video command on the server.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task CommandNextVideo()
        {
            await dataHub.InvokeAsync("ButtonNextVideoAsync", hubId);
        }

        #endregion
    }
}