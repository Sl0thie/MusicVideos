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
        public static SignalRClient Current
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
        private SignalRClient()
        {
            Current = this;

            try
            {
                dataHub = new HubConnectionBuilder()
                    .WithUrl("http://192.168.0.6:888/videoHub")
                    .WithAutomaticReconnect()
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
                    database.CheckSumOfBlock(index, checksum);
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
        /// Registers the client with the server.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task RegisterAsync()
        {
            await dataHub.InvokeAsync("RegisterRemoteAsync", "123456");
        }

        /// <summary>
        /// Asks the server to start the checksum process.
        /// This process will not start if another client has already started it.
        /// If another client has started if then this client uses the previous client's process.
        /// This will reduce the network traffic and increase the servers responsiveness.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task DatabaseChecksumAsync()
        {
            await dataHub.InvokeAsync("GetOutChecksum", hubId);
        }

        /// <summary>
        /// Tell the server that this checksum block has failed. The server will then resend the data of that block.
        /// </summary>
        /// <param name="index">The start index of the block.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task FailedChecksum(int index)
        {
            await dataHub.InvokeAsync("FailedChecksumAsync", hubId, index);
        }

        #endregion

        #region Settings / Filter / Volume

        /// <summary>
        /// Ask the server for the current filter.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task GetOutFilterAsync()
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