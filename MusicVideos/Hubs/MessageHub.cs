namespace MusicVideos.Hubs
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.SignalR;
    using Newtonsoft.Json;
    using LogCore3;

    /// <summary>
    /// MessageHub provides functions via SignalR.
    /// </summary>
    public class MessageHub : Hub
    {
        #region Fields

        private const int IndexMinimum = 0;
        private const int RepeatDelay = -10;
        private const int RatingIncrementPlayed = 5;
        private const int RatingIncrementSkipped = 1;
        private const int RatingIncrementQueued = 10;

        private static readonly Dictionary<int, int> RatingHistogram = new Dictionary<int, int>();

        private static List<Genre> filter = new List<Genre>();
        private static bool isRandom = true;
        private static bool noGenre;
        private static bool showAll = true;
        private static int previousIndex;
        private static DateTime lastSongStart = DateTime.Now;
        private static int lastIndex = -1;
        private static string previousLastPlayedString = string.Empty;

        #endregion

        #region Version 2

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
                await Clients.All.SendAsync("SendMessage", id, message);
            }
            catch (Exception ex)
            {
                await ErrorAsync(ex);
            }
        }

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

        /// <summary>
        /// Gets the Video objects from the server.
        /// </summary>
        /// <param name="id">The id of the remote.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task GetVideosAsync()
        {
            try
            {
                List<Video> videos = Model.Videos.Values.ToList();

                await Clients.All.SendAsync("SendMessage", videos.Count + "Videos found.");

                foreach (var item in videos)
                {
                    await Clients.All.SendAsync("SaveVideo", JsonConvert.SerializeObject(item, Formatting.None));
                }
            }
            catch (Exception ex)
            {
                await ErrorAsync(ex);
            }
        }

        private async Task ErrorAsync(Exception ex)
        {
            Log.Info("ERROR: " + ex.Message);
            await SendErrorAsync("Hub", JsonConvert.SerializeObject(ex, Formatting.None));
        }

        #endregion

        #region Log.Infoging

        /// <summary>
        /// Logs javascript errors.
        /// These errors are sent back to the Log.Info page to centralize them from all the sources.
        /// </summary>
        /// <param name="docTitle">The Title of the page that raised the error.</param>
        /// <param name="message">The error message.</param>
        /// <param name="filename">The filename containing the error.</param>
        /// <param name="lineNo">The line number of the error.</param>
        /// <param name="colNo">The column number of the error.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task LogErrorAsync(string docTitle, string message, string filename, string lineNo, string colNo)
        {
            Log.Info(DateTime.Now.ToString("h:mm:ss.fff") + " ERROR " + docTitle);
            Log.Info("   Message:" + message);
            Log.Info("  Filename:" + filename);
            Log.Info("      Line:" + lineNo);
            Log.Info("    Column:" + colNo);

            await Clients.All.SendAsync("PrintError", docTitle, message, filename, lineNo, colNo);
        }

        /// <summary>
        /// Log message from javascript.
        /// </summary>
        /// <param name="docTitle">The Title of the page that raised the message.</param>
        /// <param name="message">The message.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task LogMessageAsync(string docTitle, string message)
        {
            Log.Info(DateTime.Now.ToString("h:mm:ss.fff") + " " + docTitle + " " + message);
            await Clients.All.SendAsync("PrintMessage", docTitle, message);
        }

        #endregion

        #region Playlist

        /// <summary>
        /// Gets the playlist from the collection.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task GetPlaylistAsync()
        {
            int totalvideos = 0;
            RatingHistogram.Clear();

            // Create RatingHistogram.
            for (int i = 0; i < 101; i++)
            {
                RatingHistogram.Add(i, 0);
            }

            int top50 = 0;
            int bot50 = 0;

            foreach (Video next in Model.Videos.Values)
            {
                RatingHistogram[next.Rating]++;
                if (next.Rating > 50)
                {
                    top50++;
                }
                else
                {
                    bot50++;
                }
            }

            for (int i = 0; i < 101; i++)
            {
                if (RatingHistogram[i] > 0)
                {
                    await LogMessageAsync("MessageHub", i + " " + RatingHistogram[i]);
                }
            }

            await LogMessageAsync("MessageHub", "Top = " + top50 + " Bottom = " + bot50);

            await Clients.All.SendAsync("ClearPlaylist");
            Model.FilteredVideoIds.Clear();

            if (showAll)
            {
                List<Video> videos = Model.Videos.Values.ToList();

                foreach (var next in videos.Where(x => x.Rating >= Model.Settings.FilterRating).OrderBy(x => x.SearchArtist).ThenBy(x => x.Title))
                {
                    await Clients.All.SendAsync("SetPlaylistItem", next.Id, next.Artist, next.Title, next.Rating);
                    Model.FilteredVideoIds.Add(next.Id);
                    totalvideos++;
                }
            }
            else if (noGenre)
            {
                List<Video> videos = Model.Videos.Values.ToList();

                foreach (var next in videos.Where(x => x.Rating >= Model.Settings.FilterRating).OrderBy(x => x.SearchArtist).ThenBy(x => x.Title))
                {
                    if (next.Genres.Count == 0)
                    {
                        await Clients.All.SendAsync("SetPlaylistItem", next.Id, next.Artist, next.Title, next.Rating);
                        Model.FilteredVideoIds.Add(next.Id);
                        totalvideos++;
                    }
                }
            }
            else
            {
                List<Video> videos = Model.Videos.Values.ToList();

                foreach (var next in videos.Where(x => x.Rating >= Model.Settings.FilterRating).OrderBy(x => x.SearchArtist).ThenBy(x => x.Title))
                {
                    foreach (Genre genre in next.Genres)
                    {
                        if (filter.Contains(genre))
                        {
                            await Clients.All.SendAsync("SetPlaylistItem", next.Id, next.Artist, next.Title, next.Rating);
                            Model.FilteredVideoIds.Add(next.Id);
                            totalvideos++;
                            break;
                        }
                    }
                }
            }

            // TODO Change original source collection to reduce operations.
            await LogMessageAsync("MessageHub", "Total Videos: " + totalvideos);
            await LogMessageAsync("MessageHub", "Filter Rating: " + Model.Settings.FilterRating);
        }

        #endregion

        #region Queue

        /// <summary>
        /// Gets the queue list from the collection.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task GetQueuelistAsync()
        {
            Log.Info("Get Queue " + Model.QueuedVideoIds.Count);

            await Clients.All.SendAsync("ClearQueuelist");

            foreach (var next in Model.QueuedVideoIds)
            {
                Video nextVideo = Model.Videos[next];
                await Clients.All.SendAsync("SetQueuelistItem", nextVideo.Id, nextVideo.Artist, nextVideo.Title);
            }
        }

        /// <summary>
        /// Adds a video to the queue.
        /// </summary>
        /// <param name="idString">Id of the Video.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task AddToQueueAsync(string idString)
        {
            int id = Convert.ToInt32(idString);
            if (!Model.QueuedVideoIds.Contains(id))
            {
                Model.QueuedVideoIds.Add(id);
                Model.Videos[id].QueuedCount++;

                Model.Videos[id].Rating = Model.Videos[id].Rating + RatingIncrementQueued;
                if (Model.Videos[id].Rating > 100)
                {
                    Model.Videos[id].Rating = 100;
                }

                Model.Videos[id].LastQueued = DateTime.Now;
                if (isRandom)
                {
                    await GetNextVideoAsync();
                }
            }
        }

        #endregion

        #region Buttons

        /// <summary>
        /// Handles the previous button.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task ButtonPrevAsync()
        {
            if (lastSongStart > DateTime.Now.AddSeconds(RepeatDelay))
            {
                previousIndex++;
            }

            previousIndex++;
            await GetNextVideoAsync();
        }

        /// <summary>
        /// Handles the play button.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task ButtonPlayAsync()
        {
            await Clients.All.SendAsync("MediaPlay");
        }

        /// <summary>
        /// Handles the pause button.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task ButtonPauseAsync()
        {
            await Clients.All.SendAsync("MediaPause");
        }

        /// <summary>
        /// Handles the next button.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task ButtonNextAsync()
        {
            await GetNextVideoAsync();
        }

        #endregion

        #region Volume

        /// <summary>
        /// Handles volume messages.
        /// </summary>
        /// <param name="value">the volume value.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task SetVolumeAsync(string value)
        {
            Model.Settings.Volume = Convert.ToInt32(value);
            await Clients.All.SendAsync("SetVolume", value);
        }

        #endregion

        #region Current Video

        /// <summary>
        /// Gets the next song to be played.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task GetNextVideoAsync()
        {
            // Get the string to use for displaying the clock.
            string clockTime = DateTime.Now.ToString("h:mm");

            // Adjust rating of previous video first.
            if (lastIndex > 0)
            {
                // Don't adjust rating of videos that have not been rated at all.
                if (Model.Videos[lastIndex].Rating != 0)
                {
                    if (lastSongStart.AddMinutes(1) > DateTime.Now)
                    {
                        Model.Videos[lastIndex].Rating = Model.Videos[lastIndex].Rating - RatingIncrementSkipped;
                        if (Model.Videos[lastIndex].Rating < 1)
                        {
                            Model.Videos[lastIndex].Rating = 1;
                        }
                    }
                    else
                    {
                        Model.Videos[lastIndex].Rating = Model.Videos[lastIndex].Rating + RatingIncrementPlayed;
                        if (Model.Videos[lastIndex].Rating > 100)
                        {
                            Model.Videos[lastIndex].Rating = 100;
                        }
                    }
                }

                Log.Info("Model.Videos[lastIndex].Rating " + Model.Videos[lastIndex].Rating);
            }

            // Check if the playlist is not playing previous videos first. Then check there are any videos in the queue. Finally if there are no videos queued then pick a random song from the filtered list.
            int nextIndex;
            if (previousIndex > IndexMinimum)
            {
                isRandom = false;
                if (previousIndex > Model.PreviousVideoIds.Count)
                {
                    previousIndex = Model.PreviousVideoIds.Count;
                }

                nextIndex = Model.PreviousVideoIds[Model.PreviousVideoIds.Count - previousIndex--];
            }
            else if (Model.QueuedVideoIds.Count > IndexMinimum)
            {
                isRandom = false;
                nextIndex = Model.QueuedVideoIds[IndexMinimum];
                Model.QueuedVideoIds.RemoveAt(IndexMinimum);

                Log.Info("Queue Count " + Model.QueuedVideoIds.Count);
            }
            else
            {
                isRandom = true;
                var rand = new Random();
                nextIndex = Model.FilteredVideoIds[rand.Next(Model.FilteredVideoIds.Count) + RatingIncrementPlayed];
            }

            // If the video is not in the previous videos then add to the previous videos.
            if (previousIndex == IndexMinimum)
            {
                Model.PreviousVideoIds.Add(nextIndex);
            }

            // Adjust the properties of the next video to be played.
            DateTime lastPlayed = Model.Videos[nextIndex].LastPlayed;
            if (lastPlayed == DateTime.MinValue)
            {
                previousLastPlayedString = "Never played before";
            }
            else
            {
                TimeSpan diff = DateTime.Now.Subtract(lastPlayed);
                int days = (int)diff.TotalDays;
                switch (days)
                {
                    case 0:
                        previousLastPlayedString = lastPlayed.ToString("d/M/yyyy") + " today";
                        break;

                    case 1:
                        previousLastPlayedString = lastPlayed.ToString("d/M/yyyy") + " yesterday";
                        break;

                    default:
                        previousLastPlayedString = lastPlayed.ToString("d/M/yyyy") + " " + days + " days ago";
                        break;
                }
            }

            Model.Videos[nextIndex].LastPlayed = DateTime.Now;
            Model.Videos[nextIndex].PlayCount++;
            lastSongStart = DateTime.Now;

            // Change the path from a physical path to a virtual one.
            string path = Model.Videos[nextIndex].Path.Substring(Model.FilesPath.Length);
            path = path.Replace(@"\", "/");
            path = Model.VirtualPath + path;

            // Send data to player to play the next video.
            await Clients.All.SendAsync("SetVideo", nextIndex, path, Model.Videos[nextIndex].Artist, Model.Videos[nextIndex].Title, clockTime);

            await Clients.All.SendAsync("NextVideo", JsonConvert.SerializeObject(Model.Videos[nextIndex], Formatting.None));

            // Store the index for when the video stops playing.
            lastIndex = nextIndex;

            // To reduce the no of file operations save only when the video changes.
            Model.SaveVideos();

            Log.Info("filterRating " + Model.Settings.FilterRating);
        }

        /// <summary>
        /// Updates the video properties.
        /// </summary>
        /// <param name="idString">The Id of the video.</param>
        /// <param name="duration">The duration of the video.</param>
        /// <param name="width">The width of the video in pixels.</param>
        /// <param name="height">The height of the video in pixels.</param>
        public void UpdateVideoProperties(string idString, string duration, string width, string height)
        {
            int id = Convert.ToInt32(idString);

            if (duration is object)
            {
                if (duration.IndexOf('.') > 0)
                {
                    int seconds = Convert.ToInt32(duration.Substring(0, duration.IndexOf('.')));
                    int milliseconds;
                    string milli = duration.Substring(duration.IndexOf('.') + 1);
                    switch (milli.Length)
                    {
                        case 0:
                            milliseconds = 0;
                            break;
                        case 1:
                            milliseconds = Convert.ToInt32(milli) * 100;
                            break;
                        case 2:
                            milliseconds = Convert.ToInt32(milli) * 10;
                            break;
                        case 3:
                            milliseconds = Convert.ToInt32(milli);
                            break;
                        default:
                            milli = milli.Substring(0, 3);
                            milliseconds = Convert.ToInt32(milli);
                            break;
                    }

                    Model.Videos[id].Duration = (seconds * 1000) + milliseconds;
                }
            }

            Model.Videos[id].VideoWidth = Convert.ToInt32(width);
            Model.Videos[id].VideoHeight = Convert.ToInt32(height);
        }

        /// <summary>
        /// Gets the details of the video.
        /// </summary>
        /// <param name="id">The Id of the video.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task GetVideoDetails(string id)
        {
            Video video = Model.Videos[Convert.ToInt32(id)];
            string genres = JsonConvert.SerializeObject(video.Genres, Formatting.None);

            DateTime released = video.Released;
            string releasedString = string.Empty;
            if (released == DateTime.MinValue)
            {
                releasedString = "Unknown";
            }
            else
            {
                TimeSpan diff = DateTime.Now.Subtract(released);
                int days = (int)diff.TotalDays;

                if (days < 365)
                {
                    releasedString = released.ToString("yyyy") + " - this year";
                }
                else if (days < 730)
                {
                    releasedString = released.ToString("yyyy") + " - last year";
                }
                else
                {
                    int year = 365;
                    int years = days / year;
                    releasedString = released.ToString("yyyy") + " - " + years + " years ago";
                }
            }

            string duration = string.Empty;
            if (video.Duration == 0)
            {
                duration = "Unknown";
            }
            else
            {
                int remainder = video.Duration;
                int minutes = 0;
                int seconds = 0;

                while (remainder > 60000)
                {
                    minutes++;
                    remainder -= 60000;
                }

                while (remainder > 1000)
                {
                    seconds++;
                    remainder -= 1000;
                }

                duration = minutes.ToString() + ":" + seconds.ToString("D2") + "." + remainder.ToString("D3");
            }

            await Clients.All.SendAsync("SetVideoDetails", duration, video.Extension, genres, previousLastPlayedString, video.Rating, releasedString);
        }

        /// <summary>
        /// Sets the genre for the video.
        /// </summary>
        /// <param name="id">id of the video.</param>
        /// <param name="genreId">the genre id of the video.</param>
        /// <param name="state">The state to change to.</param>
        public void SetGenres(string id, string genreId, string state)
        {
            if (state == "add")
            {
                Model.Videos[Convert.ToInt32(id)].Genres.Add((Genre)Convert.ToInt32(genreId));
            }
            else
            {
                Model.Videos[Convert.ToInt32(id)].Genres.Remove((Genre)Convert.ToInt32(genreId));
            }
        }

        /// <summary>
        /// Sets the rating for the video.
        /// </summary>
        /// <param name="id">The Id of the video.</param>
        /// <param name="rating">The new rating for the video.</param>
        public void SetRating(string id, string rating)
        {
            Model.Videos[Convert.ToInt32(id)].Rating = Convert.ToInt32(rating);
        }

        #endregion

        #region Filter

        /// <summary>
        /// Sets the filter genre for the playlist.
        /// </summary>
        /// <param name="id">The genre id.</param>
        /// <param name="state">The state to change to.</param>
        public void SetFilterGenre(string id, string state)
        {
            Log.Info("SetFilter() " + id + " " + state);

            if (state == "add")
            {
                filter.Add((Genre)Convert.ToInt32(id));
            }
            else
            {
                filter.Remove((Genre)Convert.ToInt32(id));
            }
        }

        /// <summary>
        /// Sets the filters details.
        /// </summary>
        /// <param name="showall">True is all videos should be shown.</param>
        /// <param name="showunrated">True is unrated should be included.</param>
        /// <param name="minRating">The minimum rating for genres to show.</param>
        public void SetFilter(string showall, string showunrated, string minRating)
        {
            if (showall == "true")
            {
                showAll = true;
                filter = new List<Genre>();
            }
            else
            {
                showAll = false;
            }

            if (showunrated == "true")
            {
                noGenre = true;
                filter = new List<Genre>();
            }
            else
            {
                noGenre = false;
            }

            Model.Settings.FilterRating = Convert.ToInt32(minRating);
        }

        #endregion
    }
}