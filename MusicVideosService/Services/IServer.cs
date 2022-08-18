namespace MusicVideosService.Services
{
    public interface IServer : IHostedService
    {
        void PlayNextVideo();

        void Play();

        void Stop();

        void Pause();

        void Previous();

        void Next();
    }
}
