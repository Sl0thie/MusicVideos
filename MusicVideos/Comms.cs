namespace MusicVideos
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using LogCore3;
    using Microsoft.AspNetCore.SignalR.Client;
    using Newtonsoft.Json;

    /// <summary>
    /// VideoConnection class.
    /// </summary>
    public class Comms
    {
        /// <summary>
        /// Gets or sets the validation id.
        /// </summary>
        public string HubId
        {
            get
            {
                return hubId;
            }

            set
            {
                hubId = value;
            }
        }

        private static HubConnection videoHub;
        private readonly Random rnd = new Random();
        private readonly Collection<string> ids = new Collection<string>();
        private int nextRemote;
        private int nextPlayer;
        private string hubId;

        /// <summary>
        /// Initializes a new instance of the <see cref="Comms"/> class.
        /// </summary>
        public Comms()
        {
            Log.Info("Comms.Comms");

            try
            {
                // Create new id for the hub.
                HubId = "H" + (nextRemote++).ToString("000") + "-" + rnd.Next(0, 10).ToString() + rnd.Next(0, 10).ToString() + rnd.Next(0, 10).ToString();
                ids.Add(HubId);

                // Switch between IIS (Publish) and IIS Express. (Debug)
                if (Debugger.IsAttached)
                {
                    videoHub = new HubConnectionBuilder()
                                .WithUrl("http://localhost:8888/videoHub")
                                .Build();
                }
                else
                {
                    videoHub = new HubConnectionBuilder()
                           .WithUrl("http://192.168.0.6:888/videoHub")
                           .Build();
                }

                videoHub.On<string, string>("SendMessage", (id, message) =>
                {
                    Log.Info($"SendMessage: {id} - {message}");
                });

                videoHub.On<string, string>("SendError", (id, json) =>
                {
                    Log.Info($"ERROR: {id} - {json}");
                });

                videoHub.On<string>("GetDatabaseChecksum", (id) =>
                {
                    _ = DS.Videos.GetDatabaseChecksumAsync();
                });

                // --------------------------------------------------------------------------------------------------
                videoHub.On<string>("SaveVideo", (video) =>
                {
                    Log.Info($"SaveVideo: - {video}");
                    _ = DS.Videos.SaveVideoAsync(JsonConvert.DeserializeObject<Video>(video));
                });

                // Initialize SignalR.
                _ = InitializeSignalRAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        #region Connection

        /// <summary>
        /// Initializes SignalR.
        /// </summary>
        private static async Task InitializeSignalRAsync()
        {
            Log.Info("Comms.InitializeSignalRAsync");

            try
            {
                await videoHub.StartAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Checks if SignalR is connected.
        /// </summary>
        /// <returns>True if connected.</returns>
        public bool IsConnected()
        {
            Log.Info("Comms.IsConnected");

            if (videoHub.State == HubConnectionState.Connected)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks the SignalR connection.
        /// </summary>
        public void CheckConnectionAsync()
        {
            Log.Info("Comms.CheckConnectionAsync");

            switch (videoHub.State)
            {
                case HubConnectionState.Connected:
                    Log.Info("VideoHub.Connected");

                    break;

                case HubConnectionState.Connecting:
                    Log.Info("VideoHub.Connecting");

                    break;

                case HubConnectionState.Disconnected:
                    Log.Info("VideoHub.Disconnected");

                    _ = InitializeSignalRAsync();

                    break;

                case HubConnectionState.Reconnecting:
                    Log.Info("VideoHub.Reconnecting");

                    break;
            }
        }

        #endregion

        #region Registration

        /// <summary>
        /// Checks if the key is valid.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key is valid.</returns>
        public bool CheckKey(string key)
        {
            Log.Info("Comms.CheckKey");

            if (key == "123456")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Get a new remote id.
        /// </summary>
        /// <returns>A remote id string.</returns>
        public string GetRemoteId()
        {
            Log.Info("Comms.GetRemoteId");

            string registration = "R" + (nextRemote++).ToString("000") + "-" + rnd.Next(0, 10).ToString() + rnd.Next(0, 10).ToString() + rnd.Next(0, 10).ToString();
            ids.Add(registration);
            return registration;
        }

        /// <summary>
        /// Gets a new player id.
        /// </summary>
        /// <returns>a player id.</returns>
        public string GetPlayerId()
        {
            Log.Info("Comms.GetPlayerId");

            string registration = "P" + (nextPlayer++).ToString("000") + "-" + rnd.Next(0, 10).ToString() + rnd.Next(0, 10).ToString() + rnd.Next(0, 10).ToString();
            ids.Add(registration);
            return registration;
        }

        /// <summary>
        /// Checks if the id is correct.
        /// </summary>
        /// <param name="id">The id to check.</param>
        /// <returns>True if the id is correct.</returns>
        public bool CheckId(string id)
        {
            Log.Info("Comms.CheckId");

            if (ids.Contains(id))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Video

        /// <summary>
        /// Tell players to pause the video.
        /// </summary>
        /// <param name="start">The time to pause.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task PauseVideoAsync(DateTime start)
        {
            Log.Info("Comms.PauseVideoAsync");

            try
            {
                await videoHub.InvokeAsync("PauseVideoAsync", HubId, start);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Calls to save a video.
        /// </summary>
        /// <param name="video">The video to save.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SaveVideoAsync(Video video)
        {
            Log.Info("Comms.SaveVideoAsync");

            try
            {
                await videoHub.InvokeAsync("SaveVideoAsync", HubId, video);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Calls to load a video.
        /// </summary>
        /// <param name="video">The video to load.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task LoadVideoAsync(Video video)
        {
            Log.Info("Comms.LoadVideoAsync");

            try
            {
                await videoHub.InvokeAsync("LoadVideoAsync", HubId, JsonConvert.SerializeObject(video, Formatting.None));
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Calls to play a video.
        /// </summary>
        /// <param name="video">The video to play.</param>
        /// <param name="start">The time to start the video.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task PlayVideoAsync(Video video, DateTime start)
        {
            Log.Info("Comms.PlayVideoAsync");

            try
            {
                await videoHub.InvokeAsync("PlayVideoAsync", HubId, JsonConvert.SerializeObject(video, Formatting.None), JsonConvert.SerializeObject(start, Formatting.None));
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        #endregion

        #region Checksum

        /// <summary>
        /// Passes the total number of videos for a simple checksum.
        /// </summary>
        /// <param name="totalVideos">The number of video in the database.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SendServerChecksumAsync(int totalVideos)
        {
            Log.Info("Comms.SendServerChecksumAsync");

            try
            {
                await videoHub.InvokeAsync("ServerChecksumAsync", HubId, totalVideos);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Send a checksum block to the clients.
        /// </summary>
        /// <param name="index">The starting index for the block.</param>
        /// <param name="checksum">The checksum for the block.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SetOutChecksum(int index, int checksum)
        {
            Log.Info("Comms.SetOutChecksum");

            await videoHub.InvokeAsync("SetOutChecksum", HubId, index, checksum);
        }

        #endregion

        #region Settings / Filter / Volume

        /// <summary>
        /// Sends the current volume to clients.
        /// </summary>
        /// <param name="volume">The current volume value.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SetOutVolumeAsync(int volume)
        {
            Log.Info("Comms.GetOutVolumeAsync");

            await videoHub.InvokeAsync("GetOutVolumeAsync", HubId, volume);
        }

        /// <summary>
        /// Sends the current filter to the clients.
        /// </summary>
        /// <param name="filter">The current filter.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SetOutFilterAsync(Filter filter)
        {
            Log.Info("Comms.SetOutFilterAsync");

            await videoHub.InvokeAsync("SetOutFilterAsync", HubId, filter);
        }

        #endregion
    }
}