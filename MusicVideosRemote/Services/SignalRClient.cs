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
    /// .
    /// Verbs
    /// Client to Server
    /// GetOut = Ask for object from server.
    /// SetOut = Save an object from server.
    /// .
    /// Server to Client.
    /// GetIn = Ask for object from client.
    /// SetIn = Save object from client.
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

                dataHub.On<string, string>("PlayVideo", (json, time) =>
                {
                    Video newVideo = JsonConvert.DeserializeObject<Video>(json);
                    NowPlayingViewModel.Current.CurrentVideo = newVideo;
                    Debug.WriteLine("PlayVideo: " + newVideo.Artist);
                });

                dataHub.On<string>("SaveVideo", async (json) =>
                {
                    Video newVideo = JsonConvert.DeserializeObject<Video>(json);

                    Debug.WriteLine($"SaveVideo: {newVideo.GetHashCode()}  {json}");

                    DataStore database = await DataStore.Instance;
                    await database.SaveVideoAsync(newVideo);
                });

                dataHub.On<string>("SaveFilter", (json) =>
                {
                    Debug.WriteLine($"SaveFilter:  {json}");
                    Filter newFilter = JsonConvert.DeserializeObject<Filter>(json);
                    Settings.Filter = newFilter;
                });

                dataHub.On<string>("SaveVolume", (json) =>
                {
                    Debug.WriteLine($"SaveVolume:  {json}");
                });

                // -----------------------------------------------------------------------------
                dataHub.On<string>("SetOutRegistrationAsync", (id) =>
                {
                    hubId = id;
                    Debug.WriteLine("Hub Registration Id: " + id);
                });

                dataHub.On<int, string>("SetOutSettingsAsync", (volume, json) =>
                {
                    Debug.WriteLine($"SetOutSettingsAsync: volume = {volume}  filter = {json}");

                    Settings.Volume = volume;
                });

                dataHub.On<string>("SetOutFilterAsync", (json) =>
                {
                    Debug.WriteLine($"SetOutFilterAsync: filter = {json}");

                    Filter newFilter = JsonConvert.DeserializeObject<Filter>(json);
                    Settings.Filter = newFilter;
                });

                dataHub.On<int, int>("SetOutChecksum", async (index, checksum) =>
                {
                    Debug.WriteLine($"SetOutChecksum: index = {index} checksum = {checksum}");

                    DataStore database = await DataStore.Instance;
                    database.Checksum(index, checksum);
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }
        }

        #region Connect / Registration / Checksum

        /// <summary>
        /// Invokes Registration and other process needed to connect with the server.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ConnectAsync()
        {
            Debug.WriteLine("SignalRClient.ConnectAsync");

            await dataHub.StartAsync();
            await RegisterAsync();
            await GetOutFilterAsync();

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

            await dataHub.InvokeAsync("GetOutChecksum", hubId);
        }

        public async Task FailedChecksumAsync(int index)
        {
            Debug.WriteLine("SignalRClient.FailedChecksumAsync");

            await dataHub.InvokeAsync("FailedChecksumAsync", hubId, index);
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

        #endregion

        #region Debugging

        /// <summary>
        /// Passes an exception to the server.
        /// </summary>
        /// <param name="ex">The exception to pass.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ErrorAsync(Exception ex)
        {
            Debug.WriteLine("SignalRClient.ErrorAsync");

            await dataHub.InvokeAsync("SetInXamarinException", hubId, JsonConvert.SerializeObject(ex, Formatting.None));
        }

        #endregion

        #region Settings/Filter

        public async Task GetOutFilterAsync()
        {
            Debug.WriteLine("SignalRClient.GetOutFilterAsync");

            await dataHub.InvokeAsync("GetOutFilterAsync", hubId);
        }

        public async Task SetInFiltersAsync()
        {
            Debug.WriteLine("SignalRClient.SetInFiltersAsync");

            if (Settings.Filter is object)
            {
                await dataHub.InvokeAsync("SetInFilter", hubId, JsonConvert.SerializeObject(Settings.Filter, Formatting.None));
            }
            else
            {
                Debug.WriteLine("Filter object is null.");
            }
        }

        /// <summary>
        /// Ask the server for the settings object.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task GetOutSettingsAsync()
        {
            Debug.WriteLine("SignalRClient.GetOutSettingsAsync");

            await dataHub.InvokeAsync("GetOutSettingsAsync", hubId);
        }

        /// <summary>
        /// Sets the settings objects on the server.
        /// </summary>
        /// <param name="volume">Sets the volume for the application.</param>
        /// <param name="filter">Sets the filter for the application.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SetInSettingsAsync(int volume, Filter filter)
        {
            Debug.WriteLine("SignalRClient.SetInSettingsAsync");

            await dataHub.InvokeAsync("SetInSettingsAsync", hubId, volume, JsonConvert.SerializeObject(filter, Formatting.None));
        }

        /// <summary>
        /// Passes a filter to the server.
        /// </summary>
        /// <param name="filter">The filter to pass.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Obsolete("Use SetInSettingsAsync instead.")]
        public async Task SendFilterAsync(Filter filter)
        {
            Debug.WriteLine("SignalRClient.SendFilterAsync");

            await dataHub.InvokeAsync("SendFilterAsync", hubId, JsonConvert.SerializeObject(filter, Formatting.None));
        }

        /// <summary>
        /// Invokes Get Filter command on the server.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Obsolete("Use GetOutSettingsAsync instead.")]
        public async Task GetFilterAsync()
        {
            Debug.WriteLine("SignalRClient.GetFilterAsync");

            await dataHub.InvokeAsync("GetFilterAsync", hubId);
        }

        #endregion

        #region Video

        /// <summary>
        /// Queues a video on the server.
        /// </summary>
        /// <param name="id">The id of the video to queue.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task QueueVideoAsync(int id)
        {
            Debug.WriteLine("SignalRClient.QueueVideoAsync");

            await dataHub.InvokeAsync("QueueVideoAsync", hubId, id.ToString());
        }

        #endregion

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

        /// <summary>
        /// Invokes the Play Video command on the server.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task CommandPlayVideo()
        {
            Debug.WriteLine("SignalRClient.CommandPlayVideo");

            await dataHub.InvokeAsync("ButtonPlayVideoAsync", hubId);
        }

        /// <summary>
        /// Invokes the pause Video command on the server.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task CommandPauseVideo()
        {
            Debug.WriteLine("SignalRClient.CommandPauseVideo");

            await dataHub.InvokeAsync("ButtonPauseVideoAsync", hubId);
        }

        #endregion
    }
}