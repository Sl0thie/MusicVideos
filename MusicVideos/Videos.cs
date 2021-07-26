namespace MusicVideos
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Threading.Tasks;
    using LogCore3;
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
        /// The increment to use when a video is canceled before it ends.
        /// </summary>
        private const int IncrementClickedThough = -1;

        /// <summary>
        /// The increment to use when the video is completed.
        /// </summary>
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

        private readonly Collection<int> videoQueue = new Collection<int>();
        private bool setTimer;
        private DateTime lastStart;
        private Video lastVideo;

        /// <summary>
        /// Gets or sets a value indicating whether the timer needs to be set. (video started with a 0 duration).
        /// </summary>
        public bool SetTimer { get => setTimer; set => setTimer = value; }

        /// <summary>
        /// Gets the VideoQueue.
        /// </summary>
        public Collection<int> VideoQueue
        {
            get
            {
                Log.Info("Videos.VideoQueue.Get");

                return videoQueue;
            }
        }

        /// <summary>
        /// Gets or sets the LastStart. The time the last video started.
        /// </summary>
        public DateTime LastStart
        {
            get
            {
                Log.Info("Videos.LastStart.Get");

                return lastStart;
            }

            set
            {
                Log.Info("Videos.LastStart.Set");

                lastStart = value;
            }
        }

        /// <summary>
        /// Gets or sets the last video. The last video to be played.
        /// </summary>
        public Video LastVideo
        {
            get
            {
                Log.Info("Videos.LastVideo.Get");

                return lastVideo;
            }

            set
            {
                Log.Info("Videos.LastVideo.Set");

                lastVideo = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Videos"/> class.
        /// </summary>
        public Videos()
        {
            Log.Info("Videos.Videos");

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
            Log.Info("Videos.FilterVideos");

            Log.Info("FilterVideos Settings Details:");
            Log.Info($"Rating Min: {DS.Settings.Filter.RatingMinimum}");
            Log.Info($"Rating Max: {DS.Settings.Filter.RatingMaximum}");
            Log.Info($"Date Min: {DS.Settings.Filter.DateTimeMinimum}");
            Log.Info($"Date Max: {DS.Settings.Filter.DateTimeMaximum}");

            foreach (Genre gen in DS.Settings.Filter.Genres)
            {
                Log.Info($"Genre: {gen}");
            }

            // Clear the list first.
            filteredVideos.Clear();

            // Get the videos from the database.
            Task<List<Video>> rv = videosDatabase.QueryAsync<Video>("SELECT * FROM [Video] WHERE [Rating] BETWEEN " + DS.Settings.Filter.RatingMinimum + " AND " + DS.Settings.Filter.RatingMaximum);
            List<Video> videos = rv.Result;

            Log.Info($"FilterVideos Videos: {videos.Count}");

            // Filter the videos based on the genre's.
            if (DS.Settings.Filter.Genres.Count > 0)
            {
                // foreach (Video next in videos)
                // {
                //    Log.Info.WriteLine($"Video: {next.Artist} - {next.Title}");
                //    foreach (Genre genre in next.Genres)
                //    {
                //        Log.Info.WriteLine($"Genre: {genre}");
                //        if (DS.Settings.Filter.Genres.Contains(genre))
                //        {
                //            filteredVideos.Add(next.Id);
                //            break;
                //        }
                //    }
                // }
                foreach (var next in videos)
                {
                    filteredVideos.Add(next.Id);
                }
            }
            else
            {
                foreach (var next in videos)
                {
                    filteredVideos.Add(next.Id);
                }
            }

            AddUnplayedToQueue();

            Log.Info($"Total Filtered Videos: {filteredVideos.Count}");
        }

        /// <summary>
        /// Add all videos that have not been played to the queue.
        /// </summary>
        private void AddUnplayedToQueue()
        {
            Log.Info("Videos.AddUnplayedToQueue");

            try
            {
                // Get video from the database.
                Task<List<Video>> rv = videosDatabase.QueryAsync<Video>($"SELECT * FROM [Video] WHERE [PlayCount] = 0");
                List<Video> videos = rv.Result;

                foreach (Video next in videos)
                {
                    videoQueue.Add(next.Id);
                }
            }
            catch (Exception ex)
            {
                Log.Info("Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Plays the next video.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task PlayNextVideoAsync()
        {
            Log.Info("Videos.PlayNextVideoAsync");

            try
            {
                // Handle the last played video. If clicked though then lower the rating of the video.
                if (lastVideo is object)
                {
                    Log.Info($"Last Video: {lastVideo.Artist} - {lastVideo.Title} id: {lastVideo.Id} rating: {lastVideo.Rating}");

                    if (DateTime.Now.Subtract(lastVideo.LastPlayed).TotalSeconds < 30)
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

                    // await SaveVideoAsync(lastVideo);
                    await DS.Comms.SaveVideoAsync(lastVideo);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
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

        /// <summary>
        /// Play a video.
        /// </summary>
        /// <param name="id">The id of the video to play.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task PlayVideoAsync(int id)
        {
            Log.Info("Videos.PlayVideoAsync");

            // Get the video object from the database.
            Task<List<Video>> rv = videosDatabase.QueryAsync<Video>($"SELECT * FROM [Video] WHERE [Id] = {id}");
            List<Video> videos = rv.Result;
            Video nextVideo = videos[0];

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

            lastVideo = nextVideo;
            lastStart = DateTime.Now;

            Log.Info($"PlayVideoAsync: {nextVideo.Artist} - {nextVideo.Title} - {nextVideo.Duration}");
        }

        /// <summary>
        /// Picks a random video to play.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task PickRandomVideoAsync()
        {
            Log.Info("Videos.PickRandomVideoAsync");

            // Pick a random index from the filtered videos.
            int index = rnd.Next(0, filteredVideos.Count + 1);

            // Get the video object from the database.
            Task<List<Video>> rv = videosDatabase.QueryAsync<Video>($"SELECT * FROM [Video] WHERE [Id] = {filteredVideos[index]}");
            List<Video> videos = rv.Result;

            // Video nextVideo = FixVideoVirtualPath(videos[0]);
            Video nextVideo = videos[0];

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

            lastVideo = nextVideo;
            lastStart = DateTime.Now;

            Log.Info($"PickRandomVideoAsync: {nextVideo.Artist} - {nextVideo.Title} - {nextVideo.Duration}");
        }

        /// <summary>
        /// Save a video to the database.
        /// </summary>
        /// <param name="video">The video to be saved.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SaveVideoAsync(Video video)
        {
            Log.Info("Videos.SaveVideoAsync");

            try
            {
                Log.Info($"SaveVideoAsync: {video.Artist} - {video.Title} - {video.Duration}");

                //// TODO Move this to the initial file import.
                // video.PhysicalPath = video.Path;
                // string path = video.Path[FilesPath.Length..];
                // path = path.Replace(@"\", "/");
                // path = VirtualPath + path;
                // video.VirtualPath = path;

                // Get video from the database.
                Task<List<Video>> rv = videosDatabase.QueryAsync<Video>($"SELECT * FROM [Video] WHERE [Id] = '{video.Id}'");
                List<Video> videos = rv.Result;

                // Update the video or add it if it is not already there.
                if (videos.Count == 1)
                {
                    Log.Info("Updating");
                    await videosDatabase.UpdateAsync(video);
                }
                else
                {
                    Log.Info("Inserting");
                    await videosDatabase.InsertAsync(video);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Updates details of the video.
        /// </summary>
        /// <param name="id">The id of the video.</param>
        /// <param name="duration">The duration of the video.</param>
        /// <param name="width">The width of the video.</param>
        /// <param name="height">The height of the video.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateVideoDetailsAsync(int id, int duration, int width, int height)
        {
            Log.Info("Videos.UpdateVideoDetailsAsync");

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
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Gets the total number of videos as a simple checksum.
        /// Will be expanded at a later date.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task GetDatabaseChecksumAsync()
        {
            Log.Info("Videos.GetDatabaseChecksumAsync");

            try
            {
                await DS.Comms.SendServerChecksumAsync(GetTotalVideos());
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Loads the video file data from a json file.
        /// </summary>
        private void LoadVideos()
        {
            Log.Info("Videos.LoadVideos");

            // Get all videos from the JSON file.
            Dictionary<int, Video> videos;
            string json = File.ReadAllText(FilesPath + "\\index.json");
            videos = JsonConvert.DeserializeObject<Dictionary<int, Video>>(json);

            // Loop though each to add to the videos table.
            foreach (Video next in videos.Values)
            {
                _ = SaveVideoAsync(next);
            }
        }

        /// <summary>
        /// Gets all videos from the database.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<List<Video>> GetAllVideosAsync()
        {
            Log.Info("Videos.GetAllVideosAsync");

            try
            {
                return videosDatabase.Table<Video>().ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return null;
            }
        }

        /// <summary>
        /// Gets the total number of videos in the video table.
        /// </summary>
        /// <returns>A <see cref="int"/> of the total number of videos.</returns>
        private int GetTotalVideos()
        {
            Log.Info("Videos.GetTotalVideos");

            Task<int> rv = videosDatabase.Table<Video>().CountAsync();
            return rv.Result;
        }
    }
}