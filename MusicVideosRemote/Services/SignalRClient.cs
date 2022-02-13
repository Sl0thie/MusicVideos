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

        private readonly HubConnection dataHub;

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

                _ = dataHub.On<string, string>("PlayVideo", (json, time) =>
                  {
                      Video newVideo = JsonConvert.DeserializeObject<Video>(json);
                      NowPlayingViewModel.Current.CurrentVideo = newVideo;
                  });

                _ = dataHub.On<string>("SaveVideo", async (json) =>
                  {
                      Video newVideo = JsonConvert.DeserializeObject<Video>(json);
                      DataStore database = await DataStore.Instance;
                      _ = await database.SaveVideoAsync(newVideo);
                  });

                _ = dataHub.On<string>("SaveFilter", (json) =>
                  {
                      Filter newFilter = JsonConvert.DeserializeObject<Filter>(json);
                      Settings.Filter = newFilter;
                  });

                // -----------------------------------------------------------------------------
                _ = dataHub.On<string>("SetOutRegistrationAsync", (id) =>
                  {
                      hubId = id;
                      Debug.WriteLine("Hub Registration Id: " + id);
                  });

                _ = dataHub.On<int, string>("SetOutSettingsAsync", (volume, json) => Settings.Volume = volume);

                _ = dataHub.On<string>("SetOutFilterAsync", (json) =>
                  {
                      Filter newFilter = JsonConvert.DeserializeObject<Filter>(json);
                      Settings.Filter = newFilter;
                  });

                _ = dataHub.On<int, int>("SetOutChecksum", async (index, checksum) =>
                  {
                      DataStore database = await DataStore.Instance;
                      database.CheckSumOfBlock(index, checksum);
                  });

                _ = dataHub.On<int>("SetOutVolumeAsync", (volume) =>
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
        /// Tell the server that this checksum block has failed. The server will then resend the data of that block.
        /// </summary>
        /// <param name="index">The start index of the block.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task FailedChecksumAsync(int index)
        {
            await dataHub.InvokeAsync("FailedChecksum", hubId, index);
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
        /// Registers the client with the server.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task RegisterAsync()
        {
            await dataHub.InvokeAsync("RegisterRemoteAsync", "123456");
        }

        #endregion Connect / Registration / Checksum

        #region Settings / Filter / Volume

        /// <summary>
        /// Ask the server for the settings object.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task GetOutVolumeAsync()
        {
            if (dataHub.State == HubConnectionState.Connected)
            {
                await dataHub.InvokeAsync("GetOutVolumeAsync", hubId);
            }
            else
            {
                await dataHub.StartAsync();
                await dataHub.InvokeAsync("GetOutVolumeAsync", hubId);
            }
        }

        /// <summary>
        /// Sends the current filter to the server.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SetInFiltersAsync()
        {
            if (dataHub.State == HubConnectionState.Connected)
            {
                if (Settings.Filter is object)
                {
                    await dataHub.InvokeAsync("SetInFilter", hubId, JsonConvert.SerializeObject(Settings.Filter, Formatting.None));
                }
            }
            else
            {
                await dataHub.StartAsync();
                if (Settings.Filter is object)
                {
                    await dataHub.InvokeAsync("SetInFilter", hubId, JsonConvert.SerializeObject(Settings.Filter, Formatting.None));
                }
            }
        }

        /// <summary>
        /// Sets the volume on the server.
        /// </summary>
        /// <param name="volume">Sets the volume for the application.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SetInVolumeAsync(int volume)
        {
            if (dataHub.State == HubConnectionState.Connected)
            {
                await dataHub.InvokeAsync("SetOutVolume", hubId, volume);
            }
            else
            {
                await dataHub.StartAsync();
                await dataHub.InvokeAsync("SetOutVolume", hubId, volume);
            }
        }

        /// <summary>
        /// Ask the server for the current filter.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task GetOutFilterAsync()
        {
            if (dataHub.State == HubConnectionState.Connected)
            {
                await dataHub.InvokeAsync("GetOutFilterAsync", hubId);
            }
            else
            {
                await dataHub.StartAsync();
                await dataHub.InvokeAsync("GetOutFilterAsync", hubId);
            }
        }

        #endregion Settings / Filter / Volume

        #region Video

        /// <summary>
        /// Queues a video on the server.
        /// </summary>
        /// <param name="id">The id of the video to queue.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task QueueVideoAsync(int id)
        {
            if (dataHub.State == HubConnectionState.Connected)
            {
                await dataHub.InvokeAsync("QueueVideoAsync", hubId, id.ToString());
            }
            else
            {
                await dataHub.StartAsync();
                await dataHub.InvokeAsync("QueueVideoAsync", hubId, id.ToString());
            }
        }

        #endregion Video

        #region Commands

        /// <summary>
        /// Invokes the Next Video command on the server.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task CommandNextVideo()
        {
            if (dataHub.State == HubConnectionState.Connected)
            {
                await dataHub.InvokeAsync("ButtonNextVideoAsync", hubId);
            }
            else
            {
                await dataHub.StartAsync();
                await dataHub.InvokeAsync("ButtonNextVideoAsync", hubId);
            }
        }

        /// <summary>
        /// Invokes the pause Video command on the server.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task CommandPauseVideo()
        {
            if (dataHub.State == HubConnectionState.Connected)
            {
                await dataHub.InvokeAsync("ButtonPauseVideoAsync", hubId);
            }
            else
            {
                await dataHub.StartAsync();
                await dataHub.InvokeAsync("ButtonPauseVideoAsync", hubId);
            }
        }

        /// <summary>
        /// Invokes the Play Video command on the server.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task CommandPlayVideo()
        {
            if (dataHub.State == HubConnectionState.Connected)
            {
                await dataHub.InvokeAsync("ButtonPlayVideoAsync", hubId);
            }
            else
            {
                await dataHub.StartAsync();
                await dataHub.InvokeAsync("ButtonPlayVideoAsync", hubId);
            }
        }

        /// <summary>
        /// Invokes the Previous Video command on the server.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task CommandPreviousVideo()
        {
            if (dataHub.State == HubConnectionState.Connected)
            {
                await dataHub.InvokeAsync("ButtonPreviousVideoAsync", hubId);
            }
            else
            {
                await dataHub.StartAsync();
                await dataHub.InvokeAsync("ButtonPreviousVideoAsync", hubId);
            }
        }

        #endregion Commands
    }
}