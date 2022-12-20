namespace MusicVideosService.Services
{
    using System;
    using System.Reflection;
    using System.Text;
    using MusicVideosService.Models;
    using Serilog;
    using SQLite;

    public class DataStore : IDataStore
    {
        /// <summary>
        /// Flags for the database.
        /// </summary>
        private const SQLiteOpenFlags Flags =
            SQLiteOpenFlags.ReadWrite |
            SQLiteOpenFlags.Create |
            SQLiteOpenFlags.SharedCache;

        /// <summary>
        /// Connection to the sqlite database.
        /// Could probably use a better name.
        /// </summary>
        private readonly SQLiteAsyncConnection videosDatabase;

        public DataStore()
        {
            Log.Information("DataStore.Constructor");

            // Connect to the database.
            videosDatabase = new SQLiteAsyncConnection((string)Config.Application["DatabasePath"], Flags);

            // Delete the Video table. Uncomment this to remove all the video objects. This will force them to be uploaded again.
            // _ = videosDatabase.DropTableAsync<Video>();

            // Create the Video table if it is not already created.
            _ = videosDatabase.CreateTableAsync<Video>();

            // Create the Played table as well.
            _ = videosDatabase.CreateTableAsync<Played>();

            //CheckExistingVideos();

            // Import videos.
            Task<int> rv = GetNoOfVideosAsync();
            if (rv.Result == 0)
            {
                CheckExistingVideos();
            }
            else
            {
                _ = Config.Application.TryAdd("TotalVideos", rv.Result);
            }

            // Import new videos.
            ImportNewVideos();

            Log.Information("DataStore.Constructor finished.");
        }

        public Video SelectVideoFromId(int id)
        {
            // Get the video object from the database.
            Task<List<Video>> rv = videosDatabase.QueryAsync<Video>($"SELECT * FROM [Video] WHERE [Id] = {id}");
            List<Video> videos = rv.Result;
            return videos[0];
        }

        public Video SelectVideoFromRandomId(long id)
        {
            try
            {
                //Log.Information($"SelectVideoFromRandomId id {id}");

                // Get the video object from the database.
                Task<List<Video>> rv = videosDatabase.QueryAsync<Video>($"SELECT * FROM [Video] WHERE [RandomIndexLow] <= {id} AND [RandomIndexHigh] >= {id}");
                List<Video> videos = rv.Result;
                return videos[0];
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                return null;
            }
        }

        public async Task InsertOrUpdateVideo(Video video)
        {
            if (video is object)
            {
                try
                {
                    //Log.Information($"InsertOrUpdateVideo: {video.Artist} - {video.Title} - {video.Duration}");

                    // Get video from the database.
                    Task<List<Video>> rv = videosDatabase.QueryAsync<Video>($"SELECT * FROM [Video] WHERE [Id] = '{video.Id}'");
                    List<Video> videos = rv.Result;

                    // Update the video or add it if it is not already there.
                    if (videos.Count == 1)
                    {
                        //Log.Information("Updating");
                        _ = await videosDatabase.UpdateAsync(video);
                    }
                    else
                    {
                        //Log.Information("Inserting");
                        _ = await videosDatabase.InsertAsync(video);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message, ex);
                }
            }
        }

        public async Task UpdateVideoPropertiesAsync(int id, int duration, int width, int height)
        {
            try
            {
                // Get video from the database.
                Task<List<Video>> rv = videosDatabase.QueryAsync<Video>($"SELECT * FROM [Video] WHERE [Id] = '{id}'");
                List<Video> videos = rv.Result;
                if (videos.Count == 1)
                {
                    videos[0].Duration = duration;
                    videos[0].VideoWidth = width;
                    videos[0].VideoHeight = height;
                    await InsertOrUpdateVideo(videos[0]);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }

        private void CheckExistingVideos()
        {
            int basePathLength = ((string)Config.Application["BasePath"]).Length;

            foreach (string path in Directory.EnumerateFiles((string)Config.Application["BasePath"], "*.*", SearchOption.AllDirectories))
            {
                Log.Information($"File Path {path}");

                // Check for the artist/title split pattern.
                string filename = path.Substring(path.LastIndexOf(@"\") + 1);
                string extension = filename.Substring(filename.LastIndexOf(".") + 1).ToLower();
                filename = filename.Substring(0, filename.LastIndexOf("."));
                if (filename.IndexOf(" - ") < 0)
                {
                    Log.Information($"File Rejected (name pattern) path = {path}");
                    continue;
                }

                string artist = filename.Substring(0, filename.IndexOf(" - ")).Trim();
                string title = filename.Substring(filename.IndexOf(" - ") + 3).Trim();

                Log.Information($"Artist {artist} Title {title} extension {extension}");

                Video video = new Video
                {
                    Artist = artist.Replace("'", "''"),
                    Title = title.Replace("'", "''"),
                    Extension = extension,
                    PhysicalPath = path.Replace("'", "''"),
                };

                if (artist.Length > 4)
                {
                    if (artist.Substring(0, 4) == "The ")
                    {
                        video.SearchArtist = artist.Substring(4).Replace("'", "''");
                    }
                    else
                    {
                        video.SearchArtist = artist.Replace("'", "''");
                    }
                }
                else
                {
                    video.SearchArtist = artist.Replace("'", "''");
                }

                string subpath = video.PhysicalPath.Substring(basePathLength);
                subpath = subpath.Replace(@"\", "/");
                subpath = (string)Config.Application["VirtualPath"] + subpath;
                video.VirtualPath = subpath;

                Log.Information($"Virtual Path: {video.VirtualPath}");
                Log.Information($"Physical Path: {video.PhysicalPath}");

                // If file is not in collection already then add it.
                CheckIfFileExistInCollection(video);
            }
        }

        private void CheckIfFileExistInCollection(Video video)
        {
            try
            {
                //Log.Information($"SaveVideoAsync: {video.Artist} - {video.Title} - {video.Duration}");

                // Get video from the database.
                Task<List<Video>> rv = videosDatabase.QueryAsync<Video>($"SELECT * FROM [Video] WHERE [Artist] = '{video.Artist}' AND [Title] = '{video.Title}'");
                List<Video> videos = rv.Result;

                // Update the video or add it if it is not already there.
                if (videos.Count == 1)
                {
                    Log.Information("Exists");
                }
                else
                {
                    Log.Information("Inserting New");
                    _ = videosDatabase.InsertAsync(video);
                    _ = Config.Application.TryUpdate("TotalVideos", (int)Config.Application["TotalVideos"] + 1, (int)Config.Application["TotalVideos"]);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }

        private void ImportNewVideos()
        {
            foreach (string path in Directory.EnumerateFiles((string)Config.Application["ImportPath"], "*.*", SearchOption.TopDirectoryOnly))
            {
                Log.Information($"File Path {path}");
            }
        }

        public async Task<int> GetNoOfVideosAsync()
        {
            int rows = await videosDatabase.Table<Video>().CountAsync();
            return rows;
        }

        public async Task InsertPlayed(Played item)
        {
            try
            {
                _ = await videosDatabase.InsertAsync(item);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }
    }
}