using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace MusicVideos.Hubs
{
    /// <summary>
    /// MessageHub provides functions via SignalR.
    /// </summary>
    public class MessageHub : Hub
    {
        private static readonly List<Genre> Filter = new List<Genre>();
        private static bool isRandom = true;
        private static int previousIndex;
        private static DateTime lastSongStart = DateTime.Now;
        private static int filterRating;
        private static bool showUnrated;
        private static bool showAll = true;

        /// <summary>
        /// Gets the playlist from the collection.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task GetPlaylistAsync()
        {
            await Clients.All.SendAsync("ClearPlaylist");
            Model.FilteredVideoIds.Clear();

            if (showAll)
            {
                foreach (var next in Model.Videos)
                {
                    await Clients.All.SendAsync("SetPlaylistItem", next.Value.Id, next.Value.Artist, next.Value.Title);
                    Model.FilteredVideoIds.Add(next.Value.Id);
                }
            }
            else
            {
                foreach (var next in Model.Videos)
                {
                    if (showUnrated)
                    {
                        if (next.Value.Rating == 0)
                        {
                            await Clients.All.SendAsync("SetPlaylistItem", next.Value.Id, next.Value.Artist, next.Value.Title);
                            Model.FilteredVideoIds.Add(next.Value.Id);
                            continue;
                        }
                    }

                    if (next.Value.Rating >= filterRating)
                    {
                        foreach (Genre genre in next.Value.Genres)
                        {
                            if (Filter.Contains(genre))
                            {
                                await Clients.All.SendAsync("SetPlaylistItem", next.Value.Id, next.Value.Artist, next.Value.Title);
                                Model.FilteredVideoIds.Add(next.Value.Id);
                                continue;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the queuelist from the collection.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task GetQueuelistAsync()
        {
            await Clients.All.SendAsync("ClearQueuelist");

            foreach (var next in Model.QueuedVideoIds)
            {
                Video nextVideo = Model.Videos[next];
                await Clients.All.SendAsync("SetQueuelistItem", nextVideo.Id, nextVideo.Artist, nextVideo.Title);
            }
        }

        /// <summary>
        /// Gets the next song to be played.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task GetNextSongAsync()
        {
            string clockTime = DateTime.Now.ToString("h:mm");
            Video nextVideo = new Video();
            int nextIndex;

            if (previousIndex > 0)
            {
                isRandom = false;
                if (previousIndex > Model.PreviousVideoIds.Count)
                {
                    previousIndex = Model.PreviousVideoIds.Count;
                }

                nextIndex = Model.PreviousVideoIds[Model.PreviousVideoIds.Count - previousIndex--];
            }
            else if (Model.QueuedVideoIds.Count > 0)
            {
                isRandom = false;
                nextIndex = Model.QueuedVideoIds[0];
                Model.QueuedVideoIds.RemoveAt(0);
            }
            else
            {
                isRandom = true;
                var rand = new Random();
                nextIndex = Model.FilteredVideoIds[rand.Next(Model.FilteredVideoIds.Count) + 1];
            }

            nextVideo = Model.Videos[nextIndex];
            lastSongStart = DateTime.Now;
            if (previousIndex == 0)
            {
                Model.PreviousVideoIds.Add(nextIndex);
            }

            nextVideo.LastPlayed = DateTime.Now;
            nextVideo.PlayCount++;
            string path = nextVideo.Path.Substring(Model.FilesPath.Length);
            path = path.Replace(@"\", "/");
            path = Model.VirtualPath + path;
            await Clients.All.SendAsync("SetVideo", nextIndex, path, nextVideo.Artist, nextVideo.Title, clockTime);
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
            await Clients.All.SendAsync("SetVideoDetails", video.Duration, video.Extension, genres, video.LastPlayed.ToString("d/M/yyyy"), video.Rating, video.Released.ToString("d/M/yyyy"));
        }

        /// <summary>
        /// Adds a video to the queue.
        /// </summary>
        /// <param name="id">Id of the Video.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task AddToQueueAsync(string id)
        {
            Debug.WriteLine(id);
            Model.QueuedVideoIds.Add(Convert.ToInt32(id));

            if (isRandom)
            {
                await GetNextSongAsync();
            }
        }

        /// <summary>
        /// Handles the previous button.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task ButtonPrevAsync()
        {
            if (lastSongStart > DateTime.Now.AddSeconds(-10))
            {
                previousIndex++;
            }

            previousIndex++;
            await GetNextSongAsync();
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
            await GetNextSongAsync();
        }

        /// <summary>
        /// Handles volume messages.
        /// </summary>
        /// <param name="value">the volume value.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task SetVolumeAsync(string value)
        {
            await Clients.All.SendAsync("SetVolume", value);
        }

        /// <summary>
        /// Sets the genre for the video.
        /// </summary>
        /// <param name="id">id of the video.</param>
        /// <param name="genreId">the genre id of the video.</param>
        /// <param name="state">The state to change to.</param>
        public void SetGenres(string id, string genreId, string state)
        {
            Video video = Model.Videos[Convert.ToInt32(id)];
            if (state == "add")
            {
                video.Genres.Add((Genre)Convert.ToInt32(genreId));
            }
            else
            {
                video.Genres.Remove((Genre)Convert.ToInt32(genreId));
            }

            Model.SaveVideos();
        }

        /// <summary>
        /// Sets the filter genre for the playlist.
        /// </summary>
        /// <param name="id">The genre id.</param>
        /// <param name="state">The state to change to.</param>
        public void SetFilterGenre(string id, string state)
        {
            Debug.WriteLine("SetFilter() " + id + " " + state);

            if (state == "add")
            {
                Filter.Add((Genre)Convert.ToInt32(id));
            }
            else
            {
                Filter.Remove((Genre)Convert.ToInt32(id));
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
            }
            else
            {
                showAll = false;
            }

            if (showunrated == "true")
            {
                showUnrated = true;
            }
            else
            {
                showUnrated = false;
            }

            filterRating = Convert.ToInt32(minRating);
        }

        /// <summary>
        /// Sets the rating for the video.
        /// </summary>
        /// <param name="id">The Id of the video.</param>
        /// <param name="rating">The new rating for the video.</param>
        public void SetRatingAsync(string id, string rating)
        {
            Video video = Model.Videos[Convert.ToInt32(id)];
            video.Rating = Convert.ToInt32(rating);
            Model.SaveVideos();
        }
    }
}