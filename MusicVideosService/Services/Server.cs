namespace MusicVideosService.Services
{
    using System;
    using System.Collections.ObjectModel;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Timers;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.AspNetCore.SignalR.Client;
    using MusicVideosService.Models;
    using Serilog;

    public class Server : BackgroundService, IServer
    {
        private static System.Timers.Timer timer;
        private HubConnection hub;
        private IDataStore dataStore;
        private PlayState playState = PlayState.PlayingRandom;

        private readonly Collection<int> videoPrevious = new Collection<int>();
        private readonly Queue<int> videoQueue = new Queue<int>();
        private int currentId = -1;
        private DateTime videoStarted;
        private TimeSpan videoPaused;

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

                // Create a connnection to the SignalR hub.
                hub = new HubConnectionBuilder()
                           .WithUrl("http://192.168.0.6:930/dataHub")
                           .WithAutomaticReconnect()
                           .Build();

                _ = hub.StartAsync();

                timer = new System.Timers.Timer(60000 * 60);
                timer.Elapsed += Timer_Elapsed;
                timer.AutoReset = true;
                timer.Enabled = true;
                timer.Start();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
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

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            // Log.Information("Worker Timer_Elapsed");
            try
            {
                Log.Information($"Timer Elapsed {DateTime.Now}");
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }


        public async void PlayNextVideo(bool previousError)
        {
            if (currentId > -1)
            {
                Video previousVideo = dataStore.SelectVideoFromId(currentId);
                previousVideo.PlayCount++;
                previousVideo.PlayTime += (DateTime.Now - videoStarted).TotalMilliseconds + videoPaused.TotalMilliseconds;

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
                // Pick a random index from the filtered videos.
                int index = rnd.Next(0, (int)Config.Application["TotalVideos"] + 1);

                Video nextVideo = dataStore.SelectVideoFromId(index);

                // Check if the video's last played is past the limit.
                if (DateTime.Now.Subtract(nextVideo.LastPlayed).TotalMinutes > (int)Config.Application["MinutesBetweenReplays"])
                {
                    videoStarted = DateTime.Now;
                    currentId = nextVideo.Id;

                    await hub.InvokeAsync("ClientPlayVideo", nextVideo);

                    keepsearching = false;

                    Log.Information($"PlayRandomVideoAsync: {nextVideo.Artist} - {nextVideo.Title} - {nextVideo.Id}");
                }
            }
            while (keepsearching);
        }

        public void Next()
        {
            Log.Information("Server.Next");
        }

        public void Pause()
        {
            Log.Information("Server.Pause");
        }

        public void Play()
        {
            Log.Information("Server.Play");
        }

        public void Previous()
        {
            Log.Information("Server.Previous");
        }

        public void Stop()
        {
            Log.Information("Server.Stop");
        }
    }
}
