namespace MusicVideosService
{
    using System.Collections.Concurrent;

    public static class Config
    {
        public static ConcurrentDictionary<string, object> Application = new ConcurrentDictionary<string, object>();
    }
}
