namespace MusicVideosService.Services
{
    using System.Collections.ObjectModel;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Timers;

    using Microsoft.AspNetCore.SignalR;
    using Microsoft.AspNetCore.SignalR.Client;

    using Serilog;

    using MusicVideosService.Models;

    public class Server : BackgroundService, IServer
    {
        private HubConnection hub;
        private IDataStore dataStore;
        private System.Timers.Timer timer;
        private PlayState playState = PlayState.PlayingRandom;

        private readonly Collection<int> videoPrevious = new Collection<int>();
        private readonly Queue<int> videoQueue = new Queue<int>();

        /// <summary>
        /// Used to generate random numbers.
        /// </summary>
        private readonly Random rnd = new Random();

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

                // Setup the main timer.
                timer = new System.Timers.Timer(1000);
                timer.Elapsed += Timer_Elapsed;
                timer.AutoReset = true;
                timer.Enabled = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //timer.Start();
            return Task.CompletedTask;
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            try
            {
                //ProcessNextState();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }

        public async void PlayNextVideo()
        {
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

        public async Task PlayRandomVideoAsync()
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
                    await hub.InvokeAsync("ClientPlayVideo", nextVideo);

                    if (nextVideo.Duration > 0)
                    {
                        timer.Stop();
                        timer.Interval = nextVideo.Duration;
                        timer.Start();
                    }
                    else
                    {
                        timer.Stop();
                        timer.Interval = 1000;
                        timer.Start();
                    }

                    nextVideo.LastPlayed = DateTime.Now;
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
