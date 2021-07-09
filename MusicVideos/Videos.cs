namespace MusicVideos
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
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


        private readonly Random rnd = new Random();


        private SQLiteAsyncConnection videosDatabase;
        private List<int> FilteredVideos = new List<int>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Videos"/> class.
        /// </summary>
        public Videos()
        {
            videosDatabase = new SQLiteAsyncConnection(Path.Combine(FilesPath, VideoDatabaseFilename), Flags);

            // _ = videosDatabase.DropTableAsync<Video>();

            _ = videosDatabase.CreateTableAsync<Video>();

            if (GetTotalVideos() == 0)
            {
                LoadVideos();
            }

            Debug.WriteLine("Total Videos: " + GetTotalVideos());


            FilterVideos();
        }

        /// <summary>
        /// Loads the video file data from a json file.
        /// </summary>
        private void LoadVideos()
        {
            Dictionary<int, Video> videos = new Dictionary<int, Video>();
            string json = File.ReadAllText(FilesPath + "\\index.json");
            videos = JsonConvert.DeserializeObject<Dictionary<int, Video>>(json);

            foreach (Video next in videos.Values)
            {
                SaveVideoAsync(next);
            }
        }

        private void FilterVideos()
        {
            Debug.WriteLine($"FilterVideos: Max: {DS.Settings.Filter.RatingMaximum} Min: {DS.Settings.Filter.RatingMinimum}");

            FilteredVideos.Clear();
            Task<List<Video>> rv = videosDatabase.QueryAsync<Video>("SELECT * FROM [Video] WHERE [Rating] <= " + DS.Settings.Filter.RatingMaximum + " AND [Rating] >= " + DS.Settings.Filter.RatingMinimum);
            List<Video> videos = rv.Result;

            foreach (var next in videos)
            {
                FilteredVideos.Add(next.Id);

                //foreach (Genre genre in next.Genres)
                //{
                //    if (DS.Settings.Filter.Genres.Contains(genre))
                //    {
                //        FilteredVideos.Add(next.Id);
                //        break;
                //    }
                //}
            }

            Debug.WriteLine($"Total Filtered: {FilteredVideos.Count}");
        }

        private int GetTotalVideos()
        {
            Task<int> rv = videosDatabase.Table<Video>().CountAsync();
            return rv.Result;
        }

        public async Task PickRandomVideoAsync()
        {
            int index = rnd.Next(0, FilteredVideos.Count + 1);

            Task<List<Video>> rv = videosDatabase.QueryAsync<Video>($"SELECT * FROM [Video] WHERE [Id] = {FilteredVideos[index]}");
            List<Video> videos = rv.Result;

            Video nextVideo = videos[0];

            await DS.Comms.LoadVideoAsync(nextVideo);

            await DS.Comms.PlayVideoAsync(nextVideo, DateTime.Now.AddSeconds(2));

            Debug.WriteLine($"PickRandomVideoAsync: {nextVideo.Artist} - {nextVideo.Title} - {nextVideo.Duration}");

            DS.MainTimer.Interval = nextVideo.Duration;
            DS.MainTimer.Start();
        }

        /// <summary>
        /// Save a video to the database.
        /// </summary>
        /// <param name="video">The video to be saved.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        private Task<int> SaveVideoAsync(Video video)
        {
            try
            {
                //TODO Move this to the initial file import.
                video.PhysicalPath = video.Path;

                string path = video.Path.Substring(FilesPath.Length);
                path = path.Replace(@"\", "/");
                path = VirtualPath + path;

                video.VirtualPath = path;

                Task<List<Video>> rv = videosDatabase.QueryAsync<Video>($"SELECT * FROM [Video] WHERE [Id] = '{video.Id}'");
                List<Video> videos = rv.Result;

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
    }
}