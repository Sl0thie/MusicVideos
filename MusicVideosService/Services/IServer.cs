namespace MusicVideosService.Services
{
    public interface IServer : IHostedService
    {
        void PlayNextVideo(bool playedInFull, bool previousError);
    }
}
