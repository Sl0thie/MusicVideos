using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;

namespace MusicVideos
{
    /// <summary>
    /// Model object to hold the data.
    /// </summary>
    public static class Model
    {
        /// <summary>
        /// Physical path to the directory holding the video files.
        /// </summary>
        public const string FilesPath = @"E:\More Music Videos";

        /// <summary>
        /// The virutal path of the directory holding the video files.
        /// </summary>
        public const string VirtualPath = @"/Virtual/Music Videos";

        private static Dictionary<int, Video> videos = new Dictionary<int, Video>();

        /// <summary>
        /// Gets the data related to the video files in the collection.
        /// </summary>
        public static Dictionary<int, Video> Videos
        {
            get { return videos; }
            private set { videos = value; }
        }

        /// <summary>
        /// Gets the list of queued videos.
        /// </summary>
        public static Collection<int> QueuedVideoIds { get; } = new Collection<int>();

        /// <summary>
        /// Gets the list of previous videos.
        /// </summary>
        public static Collection<int> PreviousVideoIds { get; } = new Collection<int>();

        /// <summary>
        /// Gets the list of filtered videos.
        /// </summary>
        public static Collection<int> FilteredVideoIds { get; } = new Collection<int>();

        /// <summary>
        /// Loads the video file data from a json file.
        /// </summary>
        public static void LoadVideos()
        {
            string json = File.ReadAllText(FilesPath + "\\index.json");
            videos = JsonConvert.DeserializeObject<Dictionary<int, Video>>(json);

            foreach (Video next in videos.Values)
            {
                FilteredVideoIds.Add(next.Id);
            }
        }

        /// <summary>
        /// Saves the video file data to disk.
        /// </summary>
        public static void SaveVideos()
        {
            string json = JsonConvert.SerializeObject(Videos, Formatting.None);
            File.WriteAllText(FilesPath + "\\index.json", json);
        }
    }
}