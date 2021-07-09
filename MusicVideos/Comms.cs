namespace MusicVideos
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.SignalR.Client;
    using Newtonsoft.Json;

    /// <summary>
    /// VideoConnection class.
    /// </summary>
    public class Comms
    {
        private static HubConnection videoHub;
        private readonly Random rnd = new Random();
        private readonly Collection<string> Ids = new Collection<string>();
        private int nextRemote;
        private int nextPlayer;

        public string HubId;

        /// <summary>
        /// Initializes a new instance of the <see cref="Comms"/> class.
        /// </summary>
        public Comms()
        {
            Debug.WriteLine("VideoConnection constructor");

            HubId = "H" + (nextRemote++).ToString("000") + "-" + rnd.Next(0, 10).ToString() + rnd.Next(0, 10).ToString() + rnd.Next(0, 10).ToString();
            Ids.Add(HubId);

            videoHub = new HubConnectionBuilder()
                        .WithUrl("http://127.0.0.1:8888/videoHub")
                        .Build();

            Debug.WriteLine("VideoConnection constructor finished");
        }

        public async Task StartConnectionAsync()
        {
            await videoHub.StartAsync();
        }

        public async Task CheckConnectionAsync()
        {
            switch (videoHub.State)
            {
                case HubConnectionState.Connected:
                    Debug.WriteLine("VideoHub.Connected");
                    DS.MainTimer.Start();
                    break;

                case HubConnectionState.Connecting:
                    Debug.WriteLine("VideoHub.Connecting");

                    TimelineItem nextItem = new TimelineItem();
                    nextItem.Timestamp = DateTime.Now.AddSeconds(5);
                    nextItem.ActionItem = () =>
                    {
                        DS.Comms.CheckConnectionAsync();
                    };
                    DS.AddTimelineItem(nextItem);

                    break;

                case HubConnectionState.Disconnected:
                    Debug.WriteLine("VideoHub.Disconnected");

                    videoHub = new HubConnectionBuilder()
                        .WithUrl("http://localhost:8888/videoHub")
                        .Build();

                    videoHub.On<string, string>("SendMessage", (id, message) =>
                    {
                        Debug.WriteLine($"SendMessage: {id} - {message}");
                    });

                    videoHub.On<string, string>("SendError", (id, json) =>
                    {
                        Debug.WriteLine($"ERROR: {id} - {json}");
                    });

                    await videoHub.StartAsync();

                    TimelineItem nextItem2 = new TimelineItem();
                    nextItem2.Timestamp = DateTime.Now.AddSeconds(5);
                    nextItem2.ActionItem = () =>
                    {
                        DS.Comms.CheckConnectionAsync();
                    };
                    DS.AddTimelineItem(nextItem2);

                    break;

                case HubConnectionState.Reconnecting:
                    Debug.WriteLine("VideoHub.Reconnecting");

                    TimelineItem nextItem3 = new TimelineItem();
                    nextItem3.Timestamp = DateTime.Now.AddSeconds(5);
                    nextItem3.ActionItem = () =>
                    {
                        DS.Comms.CheckConnectionAsync();
                    };
                    DS.AddTimelineItem(nextItem3);

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
            Ids.Add(registration);
            return registration;
        }

        /// <summary>
        /// Gets a new player id.
        /// </summary>
        /// <returns>a player id.</returns>
        public string GetPlayerId()
        {
            string registration = "P" + (nextPlayer++).ToString("000") + "-" + rnd.Next(0, 10).ToString() + rnd.Next(0, 10).ToString() + rnd.Next(0, 10).ToString();
            Ids.Add(registration);
            return registration;
        }

        /// <summary>
        /// Checks if the id is correct.
        /// </summary>
        /// <param name="id">The id to check.</param>
        /// <returns>True if the id is correct.</returns>
        public bool CheckId(string id)
        {
            if (Ids.Contains(id))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        public async Task LoadVideoAsync(Video video)
        {
            try
            {
                await videoHub.InvokeAsync("LoadVideoAsync", HubId, JsonConvert.SerializeObject(video, Formatting.None));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR Comms.LoadVideoAsync: {ex.Message}");
            }
        }

        public async Task PlayVideoAsync(Video video, DateTime start)
        {
            try
            {
                await videoHub.InvokeAsync("PlayVideoAsync", HubId, JsonConvert.SerializeObject(video, Formatting.None), JsonConvert.SerializeObject(start, Formatting.None));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR Comms.PlayVideoAsync: {ex.Message}");
            }
        }
    }
}