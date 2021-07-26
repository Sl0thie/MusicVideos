﻿namespace MusicVideos
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
        private static HubConnection videoHub;
        private readonly Random rnd = new Random();
        private readonly Collection<string> ids = new Collection<string>();
        private int nextRemote;
        private int nextPlayer;
        private string hubId;

        /// <summary>
        /// Gets or sets the validation id.
        /// </summary>
        public string HubId { get => hubId; set => hubId = value; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Comms"/> class.
        /// </summary>
        public Comms()
        {
            // Create new id for the hub.
            HubId = "H" + (nextRemote++).ToString("000") + "-" + rnd.Next(0, 10).ToString() + rnd.Next(0, 10).ToString() + rnd.Next(0, 10).ToString();
            ids.Add(HubId);

            // Switch between IIS (Publish) and IIS Express. (Log.Info)
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
                DS.Videos.GetDatabaseChecksumAsync();
            });

            _ = videoHub.On<string, string>("SaveVideo", (id, video) =>
              {
                  Log.Info($"SaveVideo: {id} - {video}");
                  if (DS.Comms.CheckId(id))
                  {
                      _ = DS.Videos.SaveVideoAsync(JsonConvert.DeserializeObject<Video>(video));
                  }
                  else
                  {
                      Log.Info("Id check failed.");
                  }
              });

            // Initialize SignalR.
            _ = InitializeSignalRAsync();
        }

        /// <summary>
        /// Checks if SignalR is connected.
        /// </summary>
        /// <returns>True if connected.</returns>
        public bool IsConnected()
        {
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

        #region Registration

        /// <summary>
        /// Checks if the key is valid.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key is valid.</returns>
        public bool CheckKey(string key)
        {
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

        /// <summary>
        /// Calls to save a video.
        /// </summary>
        /// <param name="video">The video to save.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SaveVideoAsync(Video video)
        {
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
            try
            {
                await videoHub.InvokeAsync("PlayVideoAsync", HubId, JsonConvert.SerializeObject(video, Formatting.None), JsonConvert.SerializeObject(start, Formatting.None));
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        public async Task SendServerChecksumAsync(int totalVideos)
        {
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
        /// Initializes SignalR.
        /// </summary>
        private static async Task InitializeSignalRAsync()
        {
            try
            {
                await videoHub.StartAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }
}