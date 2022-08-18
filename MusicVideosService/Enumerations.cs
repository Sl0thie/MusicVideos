namespace MusicVideosService
{
    using System.Runtime.CompilerServices;

    public enum PlayState
    {
        Unknown = 0,
        Stopped = 1,
        Paused = 2,
        PlayingRandom = 3,
        PlayingPlaylist = 4,
        PlayingQue = 5,
        PlayingPrevious = 6,
    }
}
