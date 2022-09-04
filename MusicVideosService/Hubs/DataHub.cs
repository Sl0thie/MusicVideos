namespace MusicVideosService.Hubs
{
    using Microsoft.AspNetCore.SignalR;
    using MusicVideosService.Models;
    using MusicVideosService.Services;
    using Serilog;

    public class DataHub : Hub
    {
        private static int currrentId;
        private readonly IDataStore dataStore;
        private readonly IServer server;

        public DataHub(IDataStore dataStore, IServer server)
        {
            try
            {
                this.dataStore = dataStore;
                this.server = server;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }

        public void ClientPlayVideo(Video video)
        {
            try
            {
                Log.Information($"VideoHub.ClientPlayVideo {video.Id}");

                currrentId = video.Id;
                _ = Clients.All.SendAsync("ClientPlayVideo", video);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }

        public void ServerUpdateVideoProprties(string videoId, string duration, string videoWidth, string videoHeight)
        {
            try
            {
                Log.Information($"VideoHub.ServerUpdateVideoProprties {videoId}");

                // Convert duration to milliseconds.
                int durationFixed = (int)(Convert.ToDouble(duration) * 1000);
                _ = dataStore.UpdateVideoPropertiesAsync(Convert.ToInt32(videoId), durationFixed, Convert.ToInt32(videoWidth), Convert.ToInt32(videoHeight));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }

        public void ServerPlayerEnded(int id)
        {
            try
            {
                Log.Information($"VideoHub.ServePlayerEnded {id} {currrentId}");

                // By switch the current id to -1 after the event,
                // the event is not fired twice if there is more than one client.
                if (id == currrentId)
                {
                    currrentId = -1;
                    server.PlayNextVideo(false);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }

        public void ServerPlayerError(int id)
        {
            try
            {
                Log.Information($"VideoHub.ServePlayerError {id} {currrentId}");

                if (id == currrentId)
                {
                    currrentId = -1;
                    server.PlayNextVideo(true);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }

        public void ServerScreenClick(int id)
        {
            try
            {
                Log.Information($"VideoHub.ServeScreenClick {id} {currrentId}");

                if (id == currrentId)
                {
                    currrentId = -1;
                    server.PlayNextVideo(false);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }

        #region Commands

        public async Task Next()
        {
            server.Next();
        }

        public async Task Pause()
        {
            server.Pause();
        }

        public async Task Play()
        {
            server.Play();
        }

        public async Task Previous()
        {
            server.Previous();
        }

        public async Task Stop()
        {
            server.Stop();
        }

        #endregion
    }
}
