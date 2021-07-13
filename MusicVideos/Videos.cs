namespace MusicVideos
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using SQLite;

    /// <summary>
    /// Videos class manages the videos.
    /// </summary>
    public class Videos
    {
        /// <summary>
        /// File name for the database.
        /// </summary>
        private const string VideoDatabaseFilename = "VideoDatabase.db3";

        /// <summary>
        /// Flags for the database.
        /// </summary>
        private const SQLiteOpenFlags Flags =
            SQLiteOpenFlags.ReadWrite |
            SQLiteOpenFlags.Create |
            SQLiteOpenFlags.SharedCache;

        /// <summary>
        /// Physical path to the directory holding the video files.
        /// </summary>
        private const string FilesPath = @"F:\Music Videos";

        /// <summary>
        /// The virtual path of the directory holding the video files.
        /// </summary>
        private const string VirtualPath = @"/Virtual/Music Videos";

        /// <summary>
        /// Used to generate random numbers.
        /// </summary>
        private readonly Random rnd = new Random();

        /// <summary>
        /// Connection to the sqlite database.
        /// Could probably use a better name.
        /// </summary>
        private SQLiteAsyncConnection videosDatabase;

        /// <summary>
        /// The current list of videos that could be played.
        /// </summary>
        private List<int> filteredVideos = new List<int>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Videos"/> class.
        /// </summary>
        public Videos()
        {
            // Connect to the database.
            videosDatabase = new SQLiteAsyncConnection(Path.Combine(FilesPath, VideoDatabaseFilename), Flags);

            // Delete the Video table. Uncomment this to remove all the video objects. This will force them to be uploaded again.
            _ = videosDatabase.DropTableAsync<Video>();

            // Create the Video table if it is not already created.
            _ = videosDatabase.CreateTableAsync<Video>();

            // If the table is empty then copy the objects from the JSON file.
            if (GetTotalVideos() == 0)
            {
                LoadVideos();
            }

            // Filter the videos based on the current filter.
            FilterVideos();
        }

        /// <summary>
        /// Filters the videos based on the settings in the filter object.
        /// </summary>
        public void FilterVideos()
        {
            // Clear the list first.
            filteredVideos.Clear();

            // Get the videos from the database.
            Task<List<Video>> rv = videosDatabase.QueryAsync<Video>("SELECT * FROM [Video] WHERE [Rating] <= " + DS.Settings.Filter.RatingMaximum + " AND [Rating] >= " + DS.Settings.Filter.RatingMinimum);
            List<Video> videos = rv.Result;

            // Filter the videos based on the genre's.
            foreach (var next in videos)
            {
                filteredVideos.Add(next.Id);

                //foreach (Genre genre in next.Genres)
                //{
                //    if (DS.Settings.Filter.Genres.Contains(genre))
                //    {
                //        filteredVideos.Add(next.Id);
                //        break;
                //    }
                //}
            }
        }

        /// <summary>
        /// Picks a random video to play.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task PickRandomVideoAsync()
        {
            // Pick a random index from the filtered videos.
            int index = rnd.Next(0, filteredVideos.Count + 1);

            // Get the video object from the database.
            Task<List<Video>> rv = videosDatabase.QueryAsync<Video>($"SELECT * FROM [Video] WHERE [Id] = {filteredVideos[index]}");
            List<Video> videos = rv.Result;
            Video nextVideo = videos[0];

            // Call for the video to be loaded.
            await DS.Comms.LoadVideoAsync(nextVideo);

            // Call for the video to be played.
            await DS.Comms.PlayVideoAsync(nextVideo, DateTime.Now.AddMilliseconds(200).ToUniversalTime());
            DS.MainTimer.Interval = nextVideo.Duration;
            DS.MainTimer.Start();

            // Update video values.
            nextVideo.LastPlayed = DateTime.Now;
            nextVideo.PlayCount++;

            // Call to update the video object on all clients and server.
            await DS.Comms.SaveVideoAsync(nextVideo);

            Debug.WriteLine($"PickRandomVideoAsync: {nextVideo.Artist} - {nextVideo.Title} - {nextVideo.Duration}");
        }

        /// <summary>
        /// Save a video to the database.
        /// </summary>
        /// <param name="video">The video to be saved.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public Task<int> SaveVideoAsync(Video video)
        {
            try
            {
                // TODO Move this to the initial file import.
                video.PhysicalPath = video.Path;
                string path = video.Path.Substring(FilesPath.Length);
                path = path.Replace(@"\", "/");
                path = VirtualPath + path;
                video.VirtualPath = path;

                // Get video from the database.
                Task<List<Video>> rv = videosDatabase.QueryAsync<Video>($"SELECT * FROM [Video] WHERE [Id] = '{video.Id}'");
                List<Video> videos = rv.Result;

                // Update the video or add it if it is not already there.
                if (videos.Count == 1)
                {
                    return videosDatabase.UpdateAsync(video);
                }
                else
                {
                    return videosDatabase.InsertAsync(video);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Loads the video file data from a json file.
        /// </summary>
        private void LoadVideos()
        {
            // Get all videos from the JSON file.
            Dictionary<int, Video> videos = new Dictionary<int, Video>();
            string json = File.ReadAllText(FilesPath + "\\index.json");
            videos = JsonConvert.DeserializeObject<Dictionary<int, Video>>(json);

            // Loop though each to add to the videos table.
            foreach (Video next in videos.Values)
            {
                SaveVideoAsync(next);
            }
        }

        /// <summary>
        /// Gets the total number of videos in the video table.
        /// </summary>
        /// <returns>A <see cref="int"/> of the total number of videos.</returns>
        private int GetTotalVideos()
        {
            Task<int> rv = videosDatabase.Table<Video>().CountAsync();
            return rv.Result;
        }
    }
}