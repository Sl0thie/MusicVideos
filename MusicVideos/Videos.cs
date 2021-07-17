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

        private const int IncrementClickedThough = -1;
        private const int IncrementPlayedThough = 5;


        /// <summary>
        /// Used to generate random numbers.
        /// </summary>
        private readonly Random rnd = new Random();

        /// <summary>
        /// Connection to the sqlite database.
        /// Could probably use a better name.
        /// </summary>
        private readonly SQLiteAsyncConnection videosDatabase;

        /// <summary>
        /// The current list of videos that could be played.
        /// </summary>
        private readonly List<int> filteredVideos = new List<int>();

        private bool setTimer;

        private List<int> videoQueue = new List<int>();

        private DateTime lastStart;
        private Video lastVideo;

        /// <summary>
        /// Gets or sets a value indicating whether the timer needs to be set. (video started with a 0 duration).
        /// </summary>
        public bool SetTimer { get => setTimer; set => setTimer = value; }

        /// <summary>
        /// Gets or sets the VideoQueue.
        /// </summary>
        public List<int> VideoQueue
        {
            get { return videoQueue; }
            set { videoQueue = value; }
        }

        /// <summary>
        /// Gets or sets the LastStart. The time the last video started.
        /// </summary>
        public DateTime LastStart
        {
            get { return lastStart; }
            set { lastStart = value; }
        }

        /// <summary>
        /// Gets or sets the last video. The last video to be played.
        /// </summary>
        public Video LastVideo
        {
            get { return lastVideo; }
            set { lastVideo = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Videos"/> class.
        /// </summary>
        public Videos()
        {
            // Connect to the database.
            videosDatabase = new SQLiteAsyncConnection(Path.Combine(FilesPath, VideoDatabaseFilename), Flags);

            // Delete the Video table. Uncomment this to remove all the video objects. This will force them to be uploaded again.
            // _ = videosDatabase.DropTableAsync<Video>();

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
            if (DS.Settings.Filter.Genres.Count > 0)
            {
                foreach (var next in videos)
                {
                    foreach (Genre genre in next.Genres)
                    {
                        if (DS.Settings.Filter.Genres.Contains(genre))
                        {
                            filteredVideos.Add(next.Id);
                            break;
                        }
                    }
                }
            }
            else
            {
                foreach (var next in videos)
                {
                    filteredVideos.Add(next.Id);
                }
            }

            Debug.WriteLine($"Total Filtered Videos: {filteredVideos.Count}");
        }

        /// <summary>
        /// Plays the next video.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task PlayNextVideoAsync()
        {
            // Handle the last played video. If clicked though then lower the rating of the video.
            if (lastVideo is object)
            {
                if (DateTime.Now.Subtract(lastVideo.LastPlayed).TotalSeconds < 20)
                {
                    lastVideo.Rating = lastVideo.Rating + IncrementClickedThough;
                    if (lastVideo.Rating < 0)
                    {
                        lastVideo.Rating = 0;
                    }
                }
                else
                {
                    lastVideo.Rating = lastVideo.Rating + IncrementPlayedThough;
                    if (lastVideo.Rating > 100)
                    {
                        lastVideo.Rating = 100;
                    }
                }

                await DS.Comms.SaveVideoAsync(lastVideo);
            }

            if (videoQueue.Count > 0)
            {
                await PlayVideoAsync(videoQueue[0]);
                videoQueue.RemoveAt(0);
            }
            else
            {
                await PickRandomVideoAsync();
            }
        }

        public async Task PlayVideoAsync(int id)
        {
            // Get the video object from the database.
            Task<List<Video>> rv = videosDatabase.QueryAsync<Video>($"SELECT * FROM [Video] WHERE [Id] = {id}");
            List<Video> videos = rv.Result;
            Video nextVideo = FixVideoVirtualPath(videos[0]);

            // Call for the video to be loaded.
            await DS.Comms.LoadVideoAsync(nextVideo);

            // Call for the video to be played.
            await DS.Comms.PlayVideoAsync(nextVideo, DateTime.Now.AddMilliseconds(200).ToUniversalTime());
            if (nextVideo.Duration > 0)
            {
                SetTimer = false;
                DS.MainTimer.Interval = nextVideo.Duration;
                DS.MainTimer.Start();
            }
            else
            {
                SetTimer = true;
            }

            Debug.WriteLine($"PlayVideoAsync: {nextVideo.Artist} - {nextVideo.Title} - {nextVideo.Duration}");
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
            Video nextVideo = FixVideoVirtualPath(videos[0]);



            // Call for the video to be loaded.
            await DS.Comms.LoadVideoAsync(nextVideo);

            // Call for the video to be played.
            await DS.Comms.PlayVideoAsync(nextVideo, DateTime.Now.AddMilliseconds(200).ToUniversalTime());
            if (nextVideo.Duration > 0)
            {
                SetTimer = false;
                DS.MainTimer.Interval = nextVideo.Duration;
                DS.MainTimer.Start();
            }
            else
            {
                SetTimer = true;
            }

            Debug.WriteLine($"PickRandomVideoAsync: {nextVideo.Artist} - {nextVideo.Title} - {nextVideo.Duration}");
        }

        private Video FixVideoVirtualPath(Video video)
        {
            //video.VirtualPath = video.VirtualPath.Replace(" ", "&nbsp;");
            video.VirtualPath = video.VirtualPath.Replace("+", "&plus;");
            return video;
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
                string path = video.Path[FilesPath.Length..];
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
        /// Updates details of the video.
        /// </summary>
        /// <param name="id">The id of the video.</param>
        /// <param name="duration">The duration of the video.</param>
        /// <param name="width">The width of the video.</param>
        /// <param name="height">The height of the video.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task UpdateVideoDetailsAsync(int id, int duration, int width, int height)
        {
            try
            {
                // Get video from the database.
                Task<List<Video>> rv = videosDatabase.QueryAsync<Video>($"SELECT * FROM [Video] WHERE [Id] = '{id}'");
                List<Video> videos = rv.Result;
                if (videos.Count == 1)
                {
                    lastVideo = videos[0];
                    videos[0].Duration = duration;
                    videos[0].VideoWidth = width;
                    videos[0].VideoHeight = height;
                    videos[0].LastPlayed = DateTime.Now;
                    videos[0].PlayCount++;
                    await DS.Comms.SaveVideoAsync(videos[0]);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Loads the video file data from a json file.
        /// </summary>
        private void LoadVideos()
        {
            // Get all videos from the JSON file.
            Dictionary<int, Video> videos;
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