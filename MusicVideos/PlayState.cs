namespace MusicVideos
{
    /// <summary>
    /// PlayState enumeration of video play states.
    /// </summary>
    public enum PlayState
    {
        /// <summary>
        /// Unknown play state.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Random video state.
        /// </summary>
        Random = 1,

        /// <summary>
        /// Queued video.
        /// </summary>
        Queued = 2,

        /// <summary>
        /// Previous played video.
        /// </summary>
        Previous = 3,
    }
}