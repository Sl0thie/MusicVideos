namespace MusicVideos.Hubs
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using LogCore3;
    using Microsoft.AspNetCore.SignalR;
    using Newtonsoft.Json;

    /// <summary>
    /// MessageHub provides functions via SignalR.
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
    public class VideoHub : Hub
    {
        #region Settings / Filter / Volume

        /// <summary>
        /// Sends the settings to the clients.
        /// </summary>
        /// <param name="id">The id to validate.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task GetOutSettingsAsync(string id)
        {
            Log.Info("VideoHub.GetOutSettingsAsync");

            try
            {
                if (DS.Comms.CheckId(id))
                {
                    // Send the settings objects to the clients.
                    await Clients.All.SendAsync("SetOutSettingsAsync", DS.Settings.Volume, JsonConvert.SerializeObject(DS.Settings.Filter, Formatting.None));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Sets the settings on the server.
        /// </summary>
        /// <param name="id">The id to validate.</param>
        /// <param name="volume">The volume value.</param>
        /// <param name="json">Serialized filter object.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SetInSettingsAsync(string id, int volume, string json)
        {
            Log.Info("VideoHub.SetInSettingsAsync");

            try
            {
                if (DS.Comms.CheckId(id))
                {
                    // Save the settings to the server.
                    Filter newFilter = JsonConvert.DeserializeObject<Filter>(json);

                    if (!DS.Settings.IsFilterEqual(newFilter, DS.Settings.Filter) || (DS.Settings.Volume != volume))
                    {
                        DS.Settings.Volume = volume;
                        DS.Settings.Filter = newFilter;
                        DS.SaveSettings();

                        // Send the new values to the clients to keep them in sync.
                        await GetOutSettingsAsync(DS.Comms.HubId);

                        // The filter has been updated so filter the videos.
                        DS.Videos.FilterVideos();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        #endregion

        #region Registration

        /// <summary>
        /// Registers a new Remote with the hub.
        /// </summary>
        /// <param name="key">The key needed to access the hub.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RegisterRemoteAsync(string key)
        {
            try
            {
                Log.Info("VideoHub.RegisterRemoteAsync");

                if (DS.Comms.CheckKey(key))
                {
                    await Clients.Caller.SendAsync("SetOutRegistrationAsync", DS.Comms.GetRemoteId());
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Registers a new Player with the hub.
        /// </summary>
        /// <param name="key">The key needed to access the hub.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RegisterPlayerAsync(string key)
        {
            try
            {
                Log.Info("VideoHub.RegisterPlayerAsync");

                if (DS.Comms.CheckKey(key))
                {
                    await Clients.Caller.SendAsync("SetOutRegistrationAsync", DS.Comms.GetPlayerId());
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        #endregion

        #region Debugging

        /// <summary>
        /// Logs javascript errors.
        /// These errors are sent back to the server to centralize them from all the sources.
        /// </summary>
        /// <param name="docTitle">The Title of the page that raised the error.</param>
        /// <param name="message">The error message.</param>
        /// <param name="filename">The filename containing the error.</param>
        /// <param name="lineNo">The line number of the error.</param>
        /// <param name="colNo">The column number of the error.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task SetInJavascriptErrorAsync(string docTitle, string message, string filename, string lineNo, string colNo)
        {
            try
            {
                Log.Info("VideoHub.SetInJavascriptErrorAsync");

                Log.Info(DateTime.Now.ToString("h:mm:ss.fff") + " ERROR " + docTitle);
                Log.Info("   Message:" + message);
                Log.Info("  Filename:" + filename);
                Log.Info("      Line:" + lineNo);
                Log.Info("    Column:" + colNo);

                await DS.Videos.VideoError();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Logs an exception from a xamarin client.
        /// </summary>
        /// <param name="id">The id to validate.</param>
        /// <param name="json">json string of the exception to log.</param>
        public void SetInXamarinException(string id, string json)
        {
            try
            {
                Log.Info("VideoHub.SetInXamarinException");

                if (DS.Comms.CheckId(id))
                {
                    Exception remoteEx = JsonConvert.DeserializeObject<Exception>(json);
                    Log.Error(remoteEx);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        #endregion

        #region Older Version

        #region Messaging

        /// <summary>
        /// Sends a message to all clients.
        /// </summary>
        /// <param name="id">The id of the sending client.</param>
        /// <param name="message">The message to send.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SendMessageAsync(string id, string message)
        {
            try
            {
                Log.Info("VideoHub.SendMessageAsync");

                Log.Info($"SendMessage: {id} - {message}");
                await Clients.All.SendAsync("SendMessage", id, message);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        #endregion

        #region Time

        /// <summary>
        /// Gets the time from the server and clients.
        /// </summary>
        /// <param name="id">A valid registration id.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task GetTimeAsync(string id)
        {
            Log.Info("VideoHub.GetTimeAsync");

            if (DS.Comms.CheckId(id))
            {
                await Clients.Others.SendAsync("GetTime");
            }
        }

        /// <summary>
        /// Sends the time to all clients.
        /// </summary>
        /// <param name="id">The client id.</param>
        /// <param name="time">The time on the client.</param>
        /// <param name="offset">The offset on the client.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SendTimeAsync(string id, string time, string offset)
        {
            Log.Info("VideoHub.SendTimeAsync");

            if (DS.Comms.CheckId(id))
            {
                await Clients.All.SendAsync("SendTime", id, time, offset);
            }
        }

        /// <summary>
        /// Sets the time offset for the client.
        /// </summary>
        /// <param name="id">The id of the client to send to.</param>
        /// <param name="offset">The time offset to apply to the client.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SetTimeOffsetAsync(string id, string offset)
        {
            Log.Info("VideoHub.SetTimeOffsetAsync");

            if (DS.Comms.CheckId(id))
            {
                await Clients.All.SendAsync("SetTimeOffset", id, offset);
            }
        }

        #endregion

        #region UI Events

        /// <summary>
        /// Responds to client screen click.
        /// </summary>
        /// <param name="id">The Id for conformation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ScreenClickAsync(string id)
        {
            Log.Info("VideoHub.ScreenClickAsync");

            try
            {
                if (DS.Comms.CheckId(id))
                {
                    await DS.Videos.PlayNextVideoAsync();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Responds to button next video.
        /// </summary>
        /// <param name="id">The Id for conformation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ButtonNextVideoAsync(string id)
        {
            Log.Info("VideoHub.ButtonNextVideoAsync");

            try
            {
                if (DS.Comms.CheckId(id))
                {
                    await DS.Videos.PlayNextVideoAsync();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Responds to button previous video.
        /// </summary>
        /// <param name="id">The Id for conformation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ButtonPreviousVideoAsync(string id)
        {
            Log.Info("VideoHub.ButtonPreviousVideoAsync");

            try
            {
                if (DS.Comms.CheckId(id))
                {
                    await DS.Videos.PlayPreviousVideoAsync();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Responds to button play video.
        /// </summary>
        /// <param name="id">The Id for conformation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ButtonPlayVideoAsync(string id)
        {
            Log.Info("VideoHub.ButtonPlayVideoAsync");

            try
            {
                if (DS.Comms.CheckId(id))
                {
                    await DS.Videos.UnpauseVideoAsync();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Responds to button pause video.
        /// </summary>
        /// <param name="id">The Id for conformation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ButtonPauseVideoAsync(string id)
        {
            Log.Info("VideoHub.ButtonPauseVideoAsync");

            try
            {
                if (DS.Comms.CheckId(id))
                {
                    await DS.Videos.PauseVideoAsync();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        #endregion

        #region Video

        /// <summary>
        /// Gets the Video objects from the server.
        /// </summary>
        /// <param name="id">The id of the remote.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task GetVideosAsync(string id)
        {
            Log.Info("VideoHub.GetVideosAsync");

            try
            {
                if (DS.Comms.CheckId(id))
                {
                    Task<List<Video>> rv = DS.Videos.GetAllVideosAsync();
                    List<Video> videos = rv.Result;

                    await Clients.All.SendAsync("SendMessage", videos.Count + "Videos found.");
                    foreach (var item in videos)
                    {
                        await Clients.All.SendAsync("SaveVideo", JsonConvert.SerializeObject(item, Formatting.None));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Calls clients to save the video to their database.
        /// </summary>
        /// <param name="id">The id for confirmation.</param>
        /// <param name="video">The video to save.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SaveVideoAsync(string id, Video video)
        {
            Log.Info("VideoHub.SaveVideoAsync");

            try
            {
                if (DS.Comms.CheckId(id))
                {
                    await Clients.All.SendAsync("SaveVideo", JsonConvert.SerializeObject(video, Formatting.None));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Calls clients to save the video to their database.
        /// </summary>
        /// <param name="id">The id for confirmation.</param>
        /// <param name="timeStr">Time string of when to pause.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task PauseVideoAsync(string id, string timeStr)
        {
            Log.Info("VideoHub.PauseVideoAsync");

            try
            {
                if (DS.Comms.CheckId(id))
                {
                    await Clients.All.SendAsync("PauseVideo", timeStr);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Calls the clients (players) to load the video so its ready to play.
        /// </summary>
        /// <param name="id">The Id for confirmation.</param>
        /// <param name="video">The video to load.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task LoadVideoAsync(string id, string video)
        {
            Log.Info("VideoHub.LoadVideoAsync");

            try
            {
                if (DS.Comms.CheckId(id))
                {
                    await Clients.All.SendAsync("LoadVideo", video);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Calls for clients to play a video.
        /// </summary>
        /// <param name="id">The Id for confirmation.</param>
        /// <param name="video">The video to play.</param>
        /// <param name="timeStr">The time to play the file.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task PlayVideoAsync(string id, string video, string timeStr)
        {
            Log.Info("VideoHub.PlayVideoAsync");

            try
            {
                if (DS.Comms.CheckId(id))
                {
                    Log.Info($"Video: {video}");
                    Log.Info($"TimeStr: {timeStr}");
                    await Clients.All.SendAsync("PlayVideo", video, timeStr);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Updates the video from the player.
        /// </summary>
        /// <param name="id">The id to be validated.</param>
        /// <param name="videoId">The id of the video.</param>
        /// <param name="duration">The duration of the video.</param>
        /// <param name="videoWidth">The width of the video.</param>
        /// <param name="videoHeight">The height of the video.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateVideoPropertiesAsync(string id, string videoId, string duration, string videoWidth, string videoHeight)
        {
            Log.Info("VideoHub.UpdateVideoPropertiesAsync");

            try
            {
                if (DS.Comms.CheckId(id))
                {
                    // Convert duration to milliseconds.
                    int durationFixed = (int)(Convert.ToDouble(duration) * 1000);

                    // Log.Info.WriteLine($"videoId: {videoId}");
                    // Log.Info.WriteLine($"duration: {duration}");
                    // Log.Info.WriteLine($"durationFixed: {durationFixed}");
                    // Log.Info.WriteLine($"videoWidth: {videoWidth}");
                    // Log.Info.WriteLine($"videoHeight: {videoHeight}");
                    if (DS.Videos.SetTimer)
                    {
                        DS.MainTimer.Interval = durationFixed;
                        DS.MainTimer.Stop();
                        DS.MainTimer.Start();
                        DS.Videos.SetTimer = false;
                    }

                    await DS.Videos.UpdateVideoDetailsAsync(Convert.ToInt32(videoId), durationFixed, Convert.ToInt32(videoWidth), Convert.ToInt32(videoHeight));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Queues a video to the Videos.Queue.
        /// </summary>
        /// <param name="id">The id to validate.</param>
        /// <param name="videoId">The video id to add to the queue.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task QueueVideoAsync(string id, string videoId)
        {
            Log.Info("VideoHub.QueueVideo");

            try
            {
                if (DS.Comms.CheckId(id))
                {
                    await DS.Videos.QueueVideoAsync(Convert.ToInt32(videoId));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        #endregion

        #region Filter

        /// <summary>
        /// Gets the filter from the server.
        /// </summary>
        /// <param name="id">The id to validate.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task GetFilterAsync(string id)
        {
            Log.Info("VideoHub.GetFilterAsync");

            try
            {
                if (DS.Comms.CheckId(id))
                {
                    await Clients.All.SendAsync("SaveFilter", JsonConvert.SerializeObject(DS.Settings.Filter, Formatting.None));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Calls clients to save the filter to their database.
        /// </summary>
        /// <param name="id">The id for confirmation.</param>
        /// <param name="filter">The filter to save.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SaveFilterAsync(string id, Filter filter)
        {
            Log.Info("VideoHub.SaveFilterAsync");

            try
            {
                if (DS.Comms.CheckId(id))
                {
                    await Clients.All.SendAsync("SaveFilter", JsonConvert.SerializeObject(filter, Formatting.None));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Send the filter to be saved centrally.
        /// </summary>
        /// <param name="id">The id to validate.</param>
        /// <param name="json">Serialized filter object.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SendFilterAsync(string id, string json)
        {
            Log.Info("VideoHub.SendFilterAsync");

            try
            {
                if (DS.Comms.CheckId(id))
                {
                    Filter newFilter = JsonConvert.DeserializeObject<Filter>(json);
                    DS.Settings.Filter = newFilter;
                    DS.SaveSettings();
                    await SaveFilterAsync(DS.Comms.HubId, DS.Settings.Filter);
                    DS.Videos.FilterVideos();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        #endregion

        /// <summary>
        /// Returns the total number of videos as a simple checksum to start with.
        /// </summary>
        /// <param name="id">The id to validate.</param>
        /// <param name="totalVideos">The total number of videos in the collection.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ServerChecksumAsync(string id, int totalVideos)
        {
            Log.Info("VideoHub.ServerChecksumAsync");

            try
            {
                if (DS.Comms.CheckId(id))
                {
                    await Clients.All.SendAsync("ServerChecksum", totalVideos.ToString());
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        #endregion
    }
}