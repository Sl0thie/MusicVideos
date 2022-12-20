namespace MusicVideosService.Hubs
{
    using Microsoft.AspNetCore.SignalR;
    using MusicVideosService.Models;
    using MusicVideosService.Services;
    using Serilog;

    public class DataHub : Hub
    {
        private readonly IDataStore dataStore;

        public DataHub(IDataStore dataStore)
        {
            try
            {
                this.dataStore = dataStore;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }

        public void HubPlayVideo(Video video)
        {
            try
            {
                //Log.Information($"DataHub.ClientPlayVideo {video.Id}");

                _ = Clients.All.SendAsync("ClientPlayVideo", video);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }

        public void HubUpdateVideoProperties(string videoId, string duration, string videoWidth, string videoHeight)
        {
            try
            {
                //Log.Information($"DataHub.ServerUpdateVideoProprties {videoId}");

                // Convert duration to milliseconds.
                int durationFixed = (int)(Convert.ToDouble(duration) * 1000);
                _ = dataStore.UpdateVideoPropertiesAsync(Convert.ToInt32(videoId), durationFixed, Convert.ToInt32(videoWidth), Convert.ToInt32(videoHeight));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }

        public void HubPlayerEnded(int id)
        {
            try
            {
                //Log.Information($"DataHub.ServePlayerEnded {id}");

                _ = Clients.All.SendAsync("ServerPlayerEnded", id);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }

        public void HubPlayerError(int id)
        {
            try
            {
                //Log.Information($"DataHub.ServePlayerError {id}");

                _ = Clients.All.SendAsync("ServerPlayerError", id);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }

        public void HubScreenClick(int id)
        {
            try
            {
                //Log.Information($"DataHub.HubScreenClick {id}");

                _ = Clients.All.SendAsync("ServerPlayerScreenClicked", id);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }
    }
}
