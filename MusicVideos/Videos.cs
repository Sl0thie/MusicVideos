namespace MusicVideos
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Text;
    using System.Threading;
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
        /// The size of the checksum block sum.
        /// </summary>
        private const int BlockSize = 40;

        /// <summary>
        /// The increment to use when a video is canceled before it ends.
        /// </summary>
        private const int IncrementClickedThough = -1;

        /// <summary>
        /// The increment to use when the video is completed.
        /// </summary>
        private const int IncrementPlayedThough = 2;

        /// <summary>
        /// The increment to use when the video is queued.
        /// </summary>
        private const int IncrementQueued = 5;

        private const int MinutesBetweenReplays = 60;

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

        private readonly Collection<int> videoPrevious = new Collection<int>();
        private readonly Collection<int> videoQueue = new Collection<int>();
        private bool setTimer;
        private DateTime lastStart;
        private Video lastVideo;
        private PlayState playState = PlayState.Unknown;
        private int previousIndex;
        private bool processingChecksum;

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

            // _ = AdjustReleaseYearAsync();
            AddUnplayedToQueue();
        }

        /// <summary>
        /// Filters the videos based on the settings in the filter object.
        /// </summary>
        public void FilterVideos()
        {
            Log.Info("Videos.FilterVideos");

            try
            {
                Log.Info("FilterVideos Settings Details:");
                Log.Info($"Rating Min: {DS.Settings.Filter.RatingMinimum}");
                Log.Info($"Rating Max: {DS.Settings.Filter.RatingMaximum}");
                Log.Info($"Released Min: {DS.Settings.Filter.ReleasedMinimum}");
                Log.Info($"Released Max: {DS.Settings.Filter.ReleasedMaximum}");

                foreach (Genre gen in DS.Settings.Filter.Genres)
                {
                    Log.Info($"Genre: {gen}");
                }

                // Clear the list first.
                filteredVideos.Clear();

                string sql = "SELECT * FROM [Video] WHERE ";
                sql += $"([Rating] BETWEEN {DS.Settings.Filter.RatingMinimum} AND {DS.Settings.Filter.RatingMaximum}) ";
                sql += $"AND ([ReleasedYear] BETWEEN {DS.Settings.Filter.ReleasedMinimum} AND {DS.Settings.Filter.ReleasedMaximum}) ";
                sql += "ORDER BY SearchArtist, Title;";

                // Get the videos from the database.
                Task<List<Video>> rv = videosDatabase.QueryAsync<Video>(sql);

                List<Video> videos = rv.Result;

                Log.Info($"FilterVideos Videos: {videos.Count}");

                foreach (var next in videos)
                {
                    filteredVideos.Add(next.Id);
                }

                Log.Info($"Total Filtered Videos: {filteredVideos.Count}");
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
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
        /// Adds a video to the queue.
        /// </summary>
        /// <param name="id">The id of the video to add.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task QueueVideoAsync(int id)
        {
            Log.Info("Videos.QueueVideoAsync " + id);

            Task<List<Video>> rv = videosDatabase.QueryAsync<Video>($"SELECT * FROM Video WHERE Id = {id}");
            List<Video> videos = rv.Result;
            Video video = videos[0];
            video.Rating += IncrementQueued;

            if (video.Rating > 100)
            {
                video.Rating = 100;
            }

            VideoQueue.Add(video.Id);
            await DS.Comms.SaveVideoAsync(video);
            await SaveVideoAsync(video);

            if (playState == PlayState.Random)
            {
                playState = PlayState.Queued;
                await PlayNextVideoAsync();
            }
        }

        /// <summary>
        /// Pause the video.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task PauseVideoAsync()
        {
            Log.Info("Videos.PauseVideoAsync");
        }

        /// <summary>
        /// restart playback of the video.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UnpauseVideoAsync()
        {
            Log.Info("Videos.UnpauseVideoAsync");
        }

        /// <summary>
        /// Plays the previous video.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task PlayPreviousVideoAsync()
        {
            Log.Info("Videos.PlayPreviousVideoAsync");

            if (videoPrevious.Count > 0)
            {
                if (playState == PlayState.Previous)
                {
                    previousIndex--;
                    previousIndex--;
                    if (previousIndex < 0)
                    {
                        previousIndex = 0;
                    }
                }
                else
                {
                    playState = PlayState.Previous;
                    previousIndex = videoPrevious.Count - 1;
                }
            }
            else
            {
                playState = PlayState.Random;
            }

            Log.Info($"previousIndex {previousIndex}");

            await PlayNextVideoAsync();
        }

        /// <summary>
        /// Plays the next video.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task PlayNextVideoAsync()
        {
            Log.Info("Videos.PlayNextVideoAsync");

            // Handle the last played video. If clicked though then lower the rating of the video.
            try
            {
                await DS.Comms.PauseVideoAsync(DateTime.Now.AddMilliseconds(200).ToUniversalTime());

                if (lastVideo is object)
                {
                    Log.Info($"Last Video: {lastVideo.Artist} - {lastVideo.Title} id: {lastVideo.Id} rating: {lastVideo.Rating}");

                    if (playState == PlayState.Previous)
                    {
                        if (previousIndex >= videoPrevious.Count)
                        {
                            playState = PlayState.Unknown;
                        }
                    }
                    else
                    {
                        videoPrevious.Add(lastVideo.Id);
                    }

                    if (DateTime.Now.Subtract(lastVideo.LastPlayed).TotalSeconds < 30)
                    {
                        lastVideo.Rating += IncrementClickedThough;
                        if (lastVideo.Rating < 0)
                        {
                            lastVideo.Rating = 0;
                        }
                    }
                    else
                    {
                        lastVideo.Rating += IncrementPlayedThough;
                        if (lastVideo.Rating > 100)
                        {
                            lastVideo.Rating = 100;
                        }
                    }

                    await DS.Comms.SaveVideoAsync(lastVideo);
                    await SaveVideoAsync(lastVideo);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            if (playState == PlayState.Previous)
            {
                await PlayVideoAsync(videoPrevious[previousIndex]);
                previousIndex++;
            }
            else if (videoQueue.Count > 0)
            {
                playState = PlayState.Queued;
                await PlayVideoAsync(videoQueue[0]);
                videoQueue.RemoveAt(0);
            }
            else
            {
                playState = PlayState.Random;
                await PickRandomVideoAsync();
            }
        }

        /// <summary>
        /// Handles video errors.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task VideoError()
        {
            Log.Info("Videos.VideoError");

            // Handle the last played video. If clicked though then lower the rating of the video.
            try
            {
                await DS.Comms.PauseVideoAsync(DateTime.Now.AddMilliseconds(200).ToUniversalTime());

                if (lastVideo is object)
                {
                    Log.Info($"Video Error: {lastVideo.Artist} - {lastVideo.Title} id: {lastVideo.Id} rating: {lastVideo.Rating}");

                    lastVideo.Errors++;
                    lastVideo.Rating = 0;
                    await DS.Comms.SaveVideoAsync(lastVideo);
                    await SaveVideoAsync(lastVideo);
                }
                else
                {
                    Log.Info("lastVideo is null");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            if (playState == PlayState.Previous)
            {
                await PlayVideoAsync(videoPrevious[previousIndex]);
                previousIndex++;
            }
            else if (videoQueue.Count > 0)
            {
                playState = PlayState.Queued;
                await PlayVideoAsync(videoQueue[0]);
                videoQueue.RemoveAt(0);
            }
            else
            {
                playState = PlayState.Random;
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

            lastVideo = nextVideo;
            lastStart = DateTime.Now.AddMilliseconds(200).ToUniversalTime();

            // Call for the video to be played.
            await DS.Comms.PlayVideoAsync(nextVideo, lastStart);
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

            Log.Info($"PlayVideoAsync: {nextVideo.Artist} - {nextVideo.Title} - {nextVideo.Duration}");
        }

        /// <summary>
        /// Picks a random video to play.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task PickRandomVideoAsync()
        {
            Log.Info("Videos.PickRandomVideoAsync");

            bool keepsearching = true;

            do
            {
                // Pick a random index from the filtered videos.
                int index = rnd.Next(0, filteredVideos.Count + 1);

                // Get the video object from the database.
                Task<List<Video>> rv = videosDatabase.QueryAsync<Video>($"SELECT * FROM [Video] WHERE [Id] = {filteredVideos[index]}");
                List<Video> videos = rv.Result;

                // Video nextVideo = FixVideoVirtualPath(videos[0]);
                Video nextVideo = videos[0];

                // Check if the video's last played is past the limit.
                if (DateTime.Now.Subtract(nextVideo.LastPlayed).TotalMinutes > MinutesBetweenReplays)
                {
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
                    keepsearching = false;

                    Log.Info($"PickRandomVideoAsync: {nextVideo.Artist} - {nextVideo.Title} - {nextVideo.Duration}");
                }
            }
            while (keepsearching);
        }

        /// <summary>
        /// Save a video to the database.
        /// </summary>
        /// <param name="video">The video to be saved.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SaveVideoAsync(Video video)
        {
            Log.Info("Videos.SaveVideoAsync");

            if (video is object)
            {
                // Get the checksum before saving the video.
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(video.Added.ToBinary()).Append(video.Album).Append(video.Artist).Append(video.Duration).Append(video.Errors);
                stringBuilder.Append(video.Extension).Append(video.Id).Append(video.LastPlayed.ToBinary()).Append(video.LastQueued.ToBinary()).Append(video.PhysicalPath);
                stringBuilder.Append(video.PlayCount).Append(video.PlayTime).Append(video.QueuedCount).Append(video.Rating).Append(video.ReleasedYear);
                stringBuilder.Append(video.SearchArtist).Append(video.Title).Append(video.VideoHeight).Append(video.VideoWidth).Append(video.VirtualPath);

                string str = stringBuilder.ToString();

                Log.Info(str);

                video.Checksum = 0;

                byte[] binary = Encoding.Unicode.GetBytes(str);

                foreach (byte b in binary)
                {
                    video.Checksum += b;
                }

                Log.Info($"{video.Artist} - {video.Title} code = {video.Checksum}");

                try
                {
                    Log.Info($"SaveVideoAsync: {video.Artist} - {video.Title} - {video.Duration}");

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
                    await SaveVideoAsync(videos[0]);
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

        /// <summary>
        /// Calculates the checksums and broadcasts them to the clients.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task BroadcastChecksumsAsync()
        {
            Log.Info("Videos.BroadcastChecksums");

            if (!processingChecksum)
            {
                processingChecksum = true;

                await Task.Run(
                async () =>
                {
                    int totalvideos = GetTotalVideos();
                    for (int i = 0; i < totalvideos; i += BlockSize)
                    {
                        // token.ThrowIfCancellationRequested();
                        await Task.Delay(5000);

                        Log.Info("Processing " + i);

                        try
                        {
                            int checksum = 0;
                            for (int j = 0; j < BlockSize; j++)
                            {
                                // Get the video object from the database.
                                Task<List<Video>> rv = videosDatabase.QueryAsync<Video>($"SELECT * FROM [Video] WHERE [Id] = {i + j}");
                                List<Video> videos = rv.Result;
                                Video nextVideo = videos[0];

                                checksum += nextVideo.Checksum;
                            }

                            await DS.Comms.SetOutChecksum(i, checksum);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex);
                        }
                    }

                    return;
                });
            }

            processingChecksum = false;
        }

        /// <summary>
        /// Sends a block of videos to the clients.
        /// </summary>
        /// <param name="index">The starting index of the block.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SendVideosBlock(int index)
        {
            Log.Info("Videos.SendVideosBlock " + index);

            await Task.Run(
                async () =>
                {
                    for (int i = 0; i < BlockSize; i++)
                    {
                        await Task.Delay(5000);

                        try
                        {
                            Task<List<Video>> rv = videosDatabase.QueryAsync<Video>($"SELECT * FROM [Video] WHERE [Id] = {index + i}");
                            List<Video> videos = rv.Result;
                            Video nextVideo = videos[0];

                            await DS.Comms.SaveVideoAsync(nextVideo);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex);
                        }
                    }

                    return;
                });
        }
    }
}