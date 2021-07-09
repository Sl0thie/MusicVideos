namespace MusicVideos.Hubs
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.SignalR;
    using Newtonsoft.Json;

    /// <summary>
    /// MessageHub provides functions via SignalR.
    /// </summary>
    public class VideoHub : Hub
    {
        #region Debugging

        /// <summary>
        /// Sends error details to other clients.
        /// </summary>
        /// <param name="id">The id of the sending client.</param>
        /// <param name="error">Serialized string of the exception.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SendErrorAsync(string id, string error)
        {
            try
            {
                await Clients.All.SendAsync("SendError", id, error);
            }
            catch (Exception ex)
            {
                await ErrorAsync(ex);
            }
        }

        public async Task ErrorAsync(Exception ex)
        {
            Debug.WriteLine("ERROR: " + ex.Message);
            await SendErrorAsync("Hub", JsonConvert.SerializeObject(ex, Formatting.None));
        }

        /// <summary>
        /// Logs javascript errors.
        /// These errors are sent back to the debug page to centralize them from all the sources.
        /// </summary>
        /// <param name="docTitle">The Title of the page that raised the error.</param>
        /// <param name="message">The error message.</param>
        /// <param name="filename">The filename containing the error.</param>
        /// <param name="lineNo">The line number of the error.</param>
        /// <param name="colNo">The column number of the error.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task LogErrorAsync(string docTitle, string message, string filename, string lineNo, string colNo)
        {
            Debug.WriteLine(DateTime.Now.ToString("h:mm:ss.fff") + " ERROR " + docTitle);
            Debug.WriteLine("   Message:" + message);
            Debug.WriteLine("  Filename:" + filename);
            Debug.WriteLine("      Line:" + lineNo);
            Debug.WriteLine("    Column:" + colNo);

            await Clients.All.SendAsync("PrintError", docTitle, message, filename, lineNo, colNo);
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
                if (DS.Comms.CheckKey(key))
                {
                    await Clients.Caller.SendAsync("SetRegistration", DS.Comms.GetRemoteId());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR RegisterRemoteAsync: {ex.Message}");
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
                if (DS.Comms.CheckKey(key))
                {
                    await Clients.Caller.SendAsync("SetRegistration", DS.Comms.GetPlayerId());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR RegisterPlayerAsync: {ex.Message}");
            }
        }

        #endregion

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
                Debug.WriteLine($"SendMessage: {id} - {message}");
                await Clients.All.SendAsync("SendMessage", id, message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR SendMessageAsync: {ex.Message}");
                //await ErrorAsync(ex);
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
            if (DS.Comms.CheckId(id))
            {
                await Clients.All.SendAsync("SetTimeOffset", id, offset);
            }
        }

        #endregion

        /// <summary>
        /// Gets the Video objects from the server.
        /// </summary>
        /// <param name="id">The id of the remote.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task GetVideosAsync(string id)
        {
            try
            {
                if (DS.Comms.CheckId(id))
                {
                    List<Video> videos = Model.Videos.Values.ToList();

                    await Clients.All.SendAsync("SendMessage", videos.Count + "Videos found.");

                    foreach (var item in videos)
                    {
                        await Clients.All.SendAsync("SaveVideo", JsonConvert.SerializeObject(item, Formatting.None));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR GetVideosAsync: {ex.Message}");
            }
        }

        public async Task LoadVideoAsync(string id, string video)
        {
            try
            {
                if (DS.Comms.CheckId(id))
                {
                    await Clients.All.SendAsync("LoadVideo", video);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR PlayVideoAsync: {ex.Message}");
            }
        }

        public async Task PlayVideoAsync(string id, string video, string time)
        {
            try
            {
                if (DS.Comms.CheckId(id))
                {
                    await Clients.All.SendAsync("PlayVideo", video, time);
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"ERROR PlayVideoAsync: {ex.Message}");
            }
        }

        public async Task ScreenClickAsync(string id)
        {
            try
            {
                Debug.WriteLine("Screen Click." + id);
                await DS.Videos.PickRandomVideoAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR PlayVideoAsync: {ex.Message}");
            }
        }
    }
}