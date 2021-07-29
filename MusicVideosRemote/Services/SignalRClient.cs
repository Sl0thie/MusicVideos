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
                Debug.WriteLine("SignalRClient.Current.Get");

                if (current is null)
                {
                    current = new SignalRClient();
                }

                return current;
            }

            set
            {
                Debug.WriteLine("SignalRClient.Current.Set");

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
            Debug.WriteLine("SignalRClient.SignalRClient");

            Current = this;

            IinitializeSignalR();
        }

        private void IinitializeSignalR()
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
                    NowPlayingViewModel.Current.CurrentVideo = newVideo;
                    Debug.WriteLine("PlayVideo: " + newVideo.Artist);
                });

                dataHub.On<string>("SaveVideo", async (json) =>
                {
                    Debug.WriteLine($"SaveVideo:  {json}");
                    Video newVideo = JsonConvert.DeserializeObject<Video>(json);
                    DataStore database = await DataStore.Instance;
                    await database.SaveVideoAsync(newVideo);
                });

                dataHub.On<string>("SaveFilter", (json) =>
                {
                    Debug.WriteLine($"SaveFilter:  {json}");
                    Filter newFilter = JsonConvert.DeserializeObject<Filter>(json);
                    FilterViewModel.Current.Filter = newFilter;
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Invokes Registration and other process needed to connect with the server.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ConnectAsync()
        {
            Debug.WriteLine("SignalRClient.ConnectAsync");

            await dataHub.StartAsync();

            await RegisterAsync();

            await GetFilterAsync();

            // await GetAllVideosAsync(); // Uncomment to update all videos from server.
            await DatabaseChecksumAsync();
        }

        /// <summary>
        /// Invokes Registration with the server.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RegisterAsync()
        {
            Debug.WriteLine("SignalRClient.RegisterAsync");

            await dataHub.InvokeAsync("RegisterRemoteAsync", "123456");
        }

        /// <summary>
        /// Invokes DatabaseChecksumAsync with the server.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DatabaseChecksumAsync()
        {
            Debug.WriteLine("SignalRClient.DatabaseChecksumAsync");

            DataStore database = await DataStore.Instance;
            int totalVideos = await database.TotalVideosAsync();

            // await dataHub.InvokeAsync("DatabaseChecksum", "");
            Debug.WriteLine($"totalVideos: {totalVideos}");
        }

        /// <summary>
        /// Invokes Get All Videos command on server.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task GetAllVideosAsync()
        {
            Debug.WriteLine("SignalRClient.GetAllVideosAsync");

            await dataHub.InvokeAsync("GetVideosAsync", hubId);
        }

        /// <summary>
        /// Invokes Get Filter command on the server.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task GetFilterAsync()
        {
            Debug.WriteLine("SignalRClient.GetFilterAsync");

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
            Debug.WriteLine("SignalRClient.ErrorAsync");

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
            Debug.WriteLine("SignalRClient.SendFilterAsync");

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
            Debug.WriteLine("SignalRClient.QueueVideoAsync");

            await dataHub.InvokeAsync("QueueVideo", hubId, id.ToString());
        }

        #region Commands

        /// <summary>
        /// Invokes the Previous Video command on the server.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task CommandPreviousVideo()
        {
            Debug.WriteLine("SignalRClient.CommandPreviousVideo");

            await dataHub.InvokeAsync("ButtonPreviousVideoAsync", hubId);
        }

        /// <summary>
        /// Invokes the Next Video command on the server.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task CommandNextVideo()
        {
            Debug.WriteLine("SignalRClient.CommandNextVideo");

            await dataHub.InvokeAsync("ButtonNextVideoAsync", hubId);
        }

        #endregion
    }
}