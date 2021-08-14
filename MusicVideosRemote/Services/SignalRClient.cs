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
            Current = this;
            IinitializeSignalR();
        }

        private void IinitializeSignalR()
        {
            try
            {
                dataHub = new HubConnectionBuilder()
                    .WithUrl("http://192.168.0.6:888/videoHub")
                    .Build();

                dataHub.On<string, string>("PlayVideo", (json, time) =>
                {
                    Video newVideo = JsonConvert.DeserializeObject<Video>(json);
                    NowPlayingViewModel.Current.CurrentVideo = newVideo;
                });

                dataHub.On<string>("SaveVideo", async (json) =>
                {
                    Video newVideo = JsonConvert.DeserializeObject<Video>(json);
                    DataStore database = await DataStore.Instance;
                    await database.SaveVideoAsync(newVideo);
                });

                dataHub.On<string>("SaveFilter", (json) =>
                {
                    Filter newFilter = JsonConvert.DeserializeObject<Filter>(json);
                    Settings.Filter = newFilter;
                });

                // -----------------------------------------------------------------------------
                dataHub.On<string>("SetOutRegistrationAsync", (id) =>
                {
                    hubId = id;
                    Debug.WriteLine("Hub Registration Id: " + id);
                });

                dataHub.On<int, string>("SetOutSettingsAsync", (volume, json) =>
                {
                    Settings.Volume = volume;
                });

                dataHub.On<string>("SetOutFilterAsync", (json) =>
                {
                    Filter newFilter = JsonConvert.DeserializeObject<Filter>(json);
                    Settings.Filter = newFilter;
                });

                dataHub.On<int, int>("SetOutChecksum", async (index, checksum) =>
                {
                    DataStore database = await DataStore.Instance;
                    database.Checksum(index, checksum);
                });

                dataHub.On<int>("SetOutVolumeAsync", (volume) =>
                {
                    Debug.WriteLine($"SetOutVolumeAsync:  {volume}");
                    Settings.Volume = volume;
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
            await dataHub.StartAsync();
            await RegisterAsync();
            await GetOutFilterAsync();
            await DatabaseChecksumAsync();
        }

        /// <summary>
        /// Invokes Registration with the server.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RegisterAsync()
        {
            await dataHub.InvokeAsync("RegisterRemoteAsync", "123456");
        }

        /// <summary>
        /// Invokes DatabaseChecksumAsync with the server.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DatabaseChecksumAsync()
        {
            await dataHub.InvokeAsync("GetOutChecksum", hubId);
        }

        /// <summary>
        /// Tell the server that this checksum block has failed. The server will then resend the data.
        /// </summary>
        /// <param name="index">The start index of the block.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task FailedChecksum(int index)
        {
            await dataHub.InvokeAsync("FailedChecksumAsync", hubId, index);
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
            await dataHub.InvokeAsync("SetInXamarinException", hubId, JsonConvert.SerializeObject(ex, Formatting.None));
        }

        #endregion

        #region Settings / Filter / Volume

        /// <summary>
        /// Ask the server for the current filter.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task GetOutFilterAsync()
        {
            await dataHub.InvokeAsync("GetOutFilterAsync", hubId);
        }

        /// <summary>
        /// Sends the current filter to the server.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SetInFiltersAsync()
        {
            if (Settings.Filter is object)
            {
                await dataHub.InvokeAsync("SetInFilter", hubId, JsonConvert.SerializeObject(Settings.Filter, Formatting.None));
            }
        }

        /// <summary>
        /// Ask the server for the settings object.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task GetOutVolumeAsync()
        {
            await dataHub.InvokeAsync("GetOutVolumeAsync", hubId);
        }

        /// <summary>
        /// Sets the volume on the server.
        /// </summary>
        /// <param name="volume">Sets the volume for the application.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SetInVolumeAsync(int volume)
        {
            await dataHub.InvokeAsync("SetOutVolume", hubId, volume);
        }

        /// <summary>
        /// Passes a filter to the server.
        /// </summary>
        /// <param name="filter">The filter to pass.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Obsolete("Use SetInSettingsAsync instead.")]
        public async Task SendFilterAsync(Filter filter)
        {
            await dataHub.InvokeAsync("SendFilterAsync", hubId, JsonConvert.SerializeObject(filter, Formatting.None));
        }

        /// <summary>
        /// Invokes Get Filter command on the server.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Obsolete("Use GetOutSettingsAsync instead.")]
        public async Task GetFilterAsync()
        {
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
            await dataHub.InvokeAsync("ButtonPreviousVideoAsync", hubId);
        }

        /// <summary>
        /// Invokes the Next Video command on the server.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task CommandNextVideo()
        {
            await dataHub.InvokeAsync("ButtonNextVideoAsync", hubId);
        }

        /// <summary>
        /// Invokes the Play Video command on the server.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task CommandPlayVideo()
        {
            await dataHub.InvokeAsync("ButtonPlayVideoAsync", hubId);
        }

        /// <summary>
        /// Invokes the pause Video command on the server.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task CommandPauseVideo()
        {
            await dataHub.InvokeAsync("ButtonPauseVideoAsync", hubId);
        }

        #endregion
    }
}