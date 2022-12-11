namespace MusicVideosService.Services
{
    using System;
    using System.Collections.ObjectModel;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Timers;

    using Microsoft.AspNetCore.Hosting.Server;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.AspNetCore.SignalR.Client;
    using MusicVideosService.Models;
    using Serilog;

    public class Server : BackgroundService, IServer
    {
        private HubConnection hub;
        private IDataStore dataStore;
        private PlayState playState = PlayState.PlayingRandom;

        private readonly Collection<int> videoPrevious = new Collection<int>();
        private readonly Queue<int> videoQueue = new Queue<int>();
        private int currentId = -1;
        private DateTime videoStarted;
        private TimeSpan videoPaused;
        private DateTime nextClick = DateTime.Now;
        private int MaxRandomIndex = 0;

        /// <summary>
        /// Used to generate random numbers.
        /// </summary>
        private readonly Random rnd = new Random();

        /// <summary>
        /// Initializes a new instance of the <see cref="Server"/> class.
        /// </summary>
        /// <param name="dataStore">The primary data store.</param>
        public Server(IDataStore dataStore)
        {
            Log.Information("Server Constructor");

            try
            {
                this.dataStore = dataStore;

                _ = RateVideosAsync();

                // Create a connection to the SignalR hub.
                hub = new HubConnectionBuilder()
                           .WithUrl("http://192.168.0.6:933/dataHub")
                           .WithAutomaticReconnect()
                           .Build();

                _ = hub.On<int>("ServerPlayerEnded", (id) => PlayerEnded(id));
                _ = hub.On<int>("ServerPlayerError", (id) => PlayerError(id));
                _ = hub.On<int>("ServerPlayerScreenClicked", (id) => PlayerScreenClicked(id));
                _ = hub.StartAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }

        private async Task RateVideosAsync()
        {
            try
            {
                Log.Information("Start RateVideosAsync");

                // Create list of video id's to access randomly.
                List<int> videoIds = new List<int>();
                int maxvideos = dataStore.GetNoOfVideosAsync().Result;
                for (int i = 1; i <= maxvideos; i++)
                {
                    videoIds.Add(i);
                }

                MaxRandomIndex = 0;

                while (videoIds.Count > 0)
                {
                    int rndid = rnd.Next(0, videoIds.Count);
                    int index = videoIds[rndid];
                    videoIds.RemoveAt(rndid);

                    Log.Information($"index {index} count {videoIds.Count}");

                    Video video = dataStore.SelectVideoFromId(index);

                    if (video.PlayCount == 0) // Song has not been played yet. Base value for an unplayed video = 10000 for now.
                    {
                        video.RandomIndexLow = MaxRandomIndex;
                        MaxRandomIndex += 10000;
                        video.RandomIndexHigh = MaxRandomIndex;
                        MaxRandomIndex += 1;
                    }
                    else
                    {
                        // Get average duration.
                        long avgDuration = video.TotalPlayTime / video.PlayCount;

                        video.Rating = (int)((double)avgDuration / (double)video.Duration * 100) + 1;

                        video.RandomIndexLow = MaxRandomIndex;
                        MaxRandomIndex += video.Rating * 100;
                        video.RandomIndexHigh = MaxRandomIndex;
                        MaxRandomIndex += 1;
                    }

                    Log.Information($"{videoIds.Count} video {video.Id} {video.Artist} - {video.Title} {video.RandomIndexLow} {video.RandomIndexHigh}");

                    await dataStore.InsertOrUpdateVideo(video);
                }

                Log.Information("Finish RateVideosAsync");

                //MaxRandomIndex -= 1;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }

        private void PlayerEnded(int id)
        {
            // By switch the current id to -1 after the event,
            // the event is not fired twice if there is more than one client.
            if (id == currentId)
            {
                //currentId = -1;
                PlayNextVideo(true, false);
            }
        }

        private void PlayerError(int id)
        {
            if (id == currentId)
            {
                //currentId = -1;
                PlayNextVideo(false, true);
            }
        }

        private void PlayerScreenClicked(int id)
        {
            Log.Information($"Server.PlayerScreenClicked {id}");

            if (nextClick < DateTime.Now)
            {
                PlayNextVideo(false, false);
                nextClick = DateTime.Now.AddMilliseconds(500);
            }
        }

        /// <summary>
        /// ExecuteAsync override is currently unused.
        /// </summary>
        /// <param name="stoppingToken">A cancellation token to stop the process.</param>
        /// <returns>A task object indicating the success.</returns>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        public async void PlayNextVideo(bool playedInFull, bool previousError)
        {
            if (currentId > -1)
            {
                Video previousVideo = dataStore.SelectVideoFromId(currentId);
                previousVideo.PlayCount++;
                previousVideo.LastPlayed = DateTime.Now;

                double totalmilliseconds = DateTime.Now.Subtract(videoStarted).TotalMilliseconds;

                // Trim the total to account for videos that get paused.
                if (totalmilliseconds > previousVideo.Duration)
                {
                    previousVideo.TotalPlayTime += previousVideo.Duration;
                }
                else
                {
                    previousVideo.TotalPlayTime += (long)totalmilliseconds;
                }

                if (playedInFull)
                {
                    previousVideo.FullPlayCount++;
                }

                if (previousError)
                {
                    previousVideo.Errors++;
                }

                if (playState != PlayState.PlayingPrevious)
                {
                    videoPrevious.Add(currentId);
                }

                await dataStore.InsertOrUpdateVideo(previousVideo);
            }

            try
            {
                switch (playState)
                {
                    case PlayState.Unknown:

                        break;

                    case PlayState.Stopped:

                        break;

                    case PlayState.Paused:

                        break;

                    case PlayState.PlayingRandom:

                        await PlayRandomVideoAsync();

                        break;

                    case PlayState.PlayingPlaylist:

                        break;

                    case PlayState.PlayingQue:

                        break;

                    case PlayState.PlayingPrevious:

                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }

        private async Task PlayRandomVideoAsync()
        {
            bool keepsearching = true;

            do
            {
                try
                {
                    // Pick a random index from the filtered videos.
                    //int index = rnd.Next(0, (int)Config.Application["TotalVideos"] + 1);
                    //Video nextVideo = dataStore.SelectVideoFromId(index);

                    int index = rnd.Next(1, MaxRandomIndex);

                    Log.Information($"PlayRandomVideoAsync index {index}");

                    Video nextVideo = dataStore.SelectVideoFromRandomId(index);

                    // Check if the video's last played is past the limit.
                    if (DateTime.Now.Subtract(nextVideo.LastPlayed).TotalMinutes > (int)Config.Application["MinutesBetweenReplays"])
                    {
                        videoStarted = DateTime.Now;
                        currentId = nextVideo.Id;

                        await hub.InvokeAsync("HubPlayVideo", nextVideo);

                        keepsearching = false;

                        Log.Information($"PlayRandomVideoAsync: {nextVideo.Artist} - {nextVideo.Title} - {nextVideo.Id}");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message, ex);
                }
            }
            while (keepsearching);
        }
    }
}
