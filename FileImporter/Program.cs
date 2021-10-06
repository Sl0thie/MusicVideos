using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using SQLite;
using System.Threading.Tasks;

[assembly: CLSCompliant(true)]
namespace FileImporter
{
    internal class Program
    {
        private const string ImportPath = @"F:\Music Videos for Import\";            // Path that the files are to be imported from.
        private const string BasePath = @"F:\Music Videos\";                         // Base path that the files are stored.
        private const string ErrorPath = @"F:\Music Videos Errors\";                 // Path that the error files are stored.
        private const string ffmpegpath = "\"C:\\Program Files\\ffmpeg\\bin\"";      // Path to ffmpeg. Enclosed in quotation marks to suit shellex.
        private const string ffmpegpathEx = @"C:\Program Files\ffmpeg\bin";          // Path to ffmpeg. Not enclosed to suit shell.
        private static Dictionary<int, Video> Videos = new Dictionary<int, Video>(); // Dictionary of Video objects related to the files.
        private const string VideoDatabaseFilename = "VideoDatabase.db3";

        /// <summary>
        /// Physical path to the directory holding the video files.
        /// </summary>
        private const string FilesPath = @"F:\Music Videos";
        /// <summary>
        /// The virtual path of the directory holding the video files.
        /// </summary>
        private const string VirtualPath = @"/Virtual/Music Videos";

        /// <summary>
        /// Flags for the database.
        /// </summary>
        private const SQLiteOpenFlags Flags =
            SQLiteOpenFlags.ReadWrite |
            SQLiteOpenFlags.Create |
            SQLiteOpenFlags.SharedCache;
        private static int NoOfVideos { get; set; }                                  // No of Videos that are in the store.
        private static DateTime LastWebSearch = DateTime.MinValue;                   // Time of the last web search. Used to limit searches per hour.

        /// <summary>
        /// Connection to the sqlite database.
        /// Could probably use a better name.
        /// </summary>
        private static SQLiteAsyncConnection videosDatabase;

        /// <summary>
        /// FilImporter preforms several tasks related to the management of a file collection of music videos. Files are first converted to formats that can be played by HTML5 Video tag. Files are then stored in a "Artist\Title" format after several properties are obtained. Then the details of these operations are recorded in a json file in the base folder.
        /// </summary>
        private static void Main()
        {
            //MoveAllFiles();
            //ConvertAllWebM();
            ProcessNewVideos();
            Console.Write("Press any key to close this window...");
            _ = Console.ReadKey();
        }

        /// <summary>
        /// Log messages to the console and debug.
        /// </summary>
        /// <param name="message"></param>
        private static void Log(string message)
        {
            Console.WriteLine(DateTime.Now.ToString("h:mm:ss") + " " + message);
            Debug.WriteLine(DateTime.Now.ToString("h:mm:ss") + " " + message);
        }

        /// <summary>
        /// Move all the files back to the import directory. </br>
        /// This function is for testing.
        /// </summary>
        private static void MoveAllFiles()
        {
            foreach (string path in Directory.EnumerateFiles(BasePath, "*.*", SearchOption.AllDirectories))
            {
                string filename = path.Substring(path.LastIndexOf(@"\") + 1);
                Log("Moving File : " + path);
                File.Move(path, ImportPath + filename);
            }
        }

        private static void ConvertAllWebM()
        {
            foreach (string path in Directory.EnumerateFiles(BasePath, "*.webm", SearchOption.AllDirectories))
            {
                Video video = new Video
                {
                    Path = path
                };
                _ = ConvertWebMtoMP4(video);
            }
        }

        /// <summary>
        /// Processes files from the import path.</summary>br>
        /// The process is additive, it can fail on a file and be restarted again after the file is fixed.
        /// </summary>
        private static void ProcessNewVideos()
        {
            // Connect to the database.
            videosDatabase = new SQLiteAsyncConnection(Path.Combine(BasePath, VideoDatabaseFilename), Flags);

            // Delete the Video table. Uncomment this to remove all the video objects. This will force them to be uploaded again.
            // _ = videosDatabase.DropTableAsync<Video>();

            // Create the Video table if it is not already created.
            _ = videosDatabase.CreateTableAsync<Video>();

            //Get the existing Video data from the index file.
            if (File.Exists(BasePath + "index.json"))
            {
                string fileContent3 = File.ReadAllText(BasePath + "index.json");
                Videos = JsonConvert.DeserializeObject<Dictionary<int, Video>>(fileContent3);
                NoOfVideos = Videos.Count;
                Log("No of Videos : " + NoOfVideos);
            }

            // Convert all videos.
            foreach (string path in Directory.EnumerateFiles(ImportPath, "*.*", SearchOption.TopDirectoryOnly))
            {
                // Check for the artist/title split pattern.
                string filename = path.Substring(path.LastIndexOf(@"\") + 1);
                string extension = filename.Substring(filename.LastIndexOf(".")).ToLower();
                filename = filename.Substring(0, filename.LastIndexOf("."));
                if (filename.IndexOf(" - ") < 0)
                {
                    Log("File Rejected (name pattern) : " + path);
                    continue;
                }

                string artist = filename.Substring(0, filename.IndexOf(" - ")).Trim();
                string title = filename.Substring(filename.IndexOf(" - ") + 3).Trim();

                Video video = new Video
                {
                    Artist = artist,
                    Title = title,
                    Extension = extension,
                    Path = path,
                    LastPlayed = DateTime.MinValue,
                    LastQueued = DateTime.MinValue,
                    Released = DateTime.MinValue,
                    Added = DateTime.Now,
                    Rating = 50,
                    PlayCount = 0,
                    PlayTime = 0,
                    QueuedCount = 0
                };
                if (artist.Length > 4)
                {
                    if (artist.Substring(0, 4) == "The ")
                    {
                        video.SearchArtist = artist.Substring(4);
                    }
                    else
                    {
                        video.SearchArtist = artist;
                    }
                }
                else
                {
                    video.SearchArtist = artist;
                }

                //Convert file if needed.
                switch (extension)
                {
                    case ".avi":
                    case ".AVI":
                        Log("File Rejected (needs conversion) : " + path);
                        continue;
                    case ".mkv":
                    case ".MKV":
                        if (!ConvertMKVtoMP4(video))
                        {
                            Log("CheckVideoTitle failed : " + video.Artist + " - " + video.Title);
                            MoveFileToErrorFolder(video);
                            continue;
                        }

                        break;
                    case ".mp4":
                    case ".MP4":
                        break;
                    case ".webm":
                    case ".WEBM":
                        //if (!ConvertWebMtoMP4(video))
                        //{
                        //    Log("CheckVideoTitle failed : " + video.Artist + " - " + video.Title);
                        //    MoveFileToErrorFolder(video);
                        //    continue;
                        //}
                        break;
                    default:
                        Log("File Rejected (unknown extension) : " + path);
                        continue;
                }

                if (!CheckVideoTitle(video))
                {
                    Log("CheckVideoTitle failed : " + video.Artist + " - " + video.Title);
                    MoveFileToErrorFolder(video);
                    continue;
                }

                if (!CheckIfFileExistInCollection(video))
                {
                    continue;
                }

                if (!GetDetailFromWeb(video))
                {
                    Log("GetDetailFromWeb failed : " + video.Artist + " - " + video.Title);
                    MoveFileToErrorFolder(video);
                    continue;
                }

                if (!MoveToFolder(video))
                {
                    Log("MoveToFolder failed : " + video.Artist + " - " + video.Title);
                    MoveFileToErrorFolder(video);
                    continue;
                }

                video.Id = ++NoOfVideos;
                // Add new video to collection.
                Videos.Add(video.Id, video);

                // Write each time due to file errors.
                string json = JsonConvert.SerializeObject(Videos, Formatting.None);
                File.WriteAllText(BasePath + "index.json", json);

                SaveVideoAsync(video);

                Log("Video Added : " + video.Artist + " - " + video.Title);
            }
        }

        private static void SaveVideoAsync(Video video)
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
                    _ = videosDatabase.UpdateAsync(video);
                }
                else
                {
                    _ = videosDatabase.InsertAsync(video);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Moves the file to the error directory.
        /// </summary>
        /// <param name="video"></param>
        private static void MoveFileToErrorFolder(Video video)
        {
            //Move the file to new folder.
            if (File.Exists(ErrorPath + video.Artist + " - " + video.Title + video.Extension))
            {
                File.Delete(ImportPath + video.Artist + " - " + video.Title + video.Extension);
            }
            else
            {
                File.Move(ImportPath + video.Artist + " - " + video.Title + video.Extension, ErrorPath + video.Artist + " - " + video.Title + video.Extension);
            }
        }

        /// <summary>
        /// Removes unwanted text from the video title.
        /// </summary>
        /// <param name="video"></param>
        /// <returns></returns>
        private static bool CheckVideoTitle(Video video)
        {
            // Check for you-tube id.
            if (video.Title.Length > 12)
            {
                if (video.Title.Substring(video.Title.Length - 12, 1) == "-")
                {
                    if (video.Title.Substring(video.Title.Length - 1, 1) != " ")
                    {
                        //Log("Title before : " + video.Title);
                        video.Title = video.Title.Substring(0, video.Title.Length - 12);
                        //Log("Title after  : " + video.Title);
                    }
                }
            }

            // Remove unwanted brackets.
            video.Title = video.Title.Replace(" (AOL Sessions)", "");
            video.Title = video.Title.Replace(" (Cover)", "");
            video.Title = video.Title.Replace(" (Explicit)", "");
            video.Title = video.Title.Replace(" (Japanese Edition)", "");
            video.Title = video.Title.Replace(" (MTV Unplugged)", "");
            video.Title = video.Title.Replace(" (music video)", "");
            video.Title = video.Title.Replace(" (Music Video)", "");
            video.Title = video.Title.Replace(" [MUSIC VIDEO]", "");
            video.Title = video.Title.Replace(" (NEW SONG 2017)", "");
            video.Title = video.Title.Replace(" (New Song 2017)", "");
            video.Title = video.Title.Replace(" (Official)", "");
            video.Title = video.Title.Replace(" [Official]", "");
            video.Title = video.Title.Replace(" (Official HD Video)", "");
            video.Title = video.Title.Replace(" [OFFICIAL MUSIC VIDEO]", "");
            video.Title = video.Title.Replace(" Official Music Video", "");
            video.Title = video.Title.Replace(" (Official Music Video)", "");
            video.Title = video.Title.Replace(" [Official Music Video]", "");
            video.Title = video.Title.Replace(" (OFFICIAL MUSIC VIDEO)", "");
            video.Title = video.Title.Replace(" (Official Audio)", "");
            video.Title = video.Title.Replace(" [Official Audio]", "");
            video.Title = video.Title.Replace(" (Official Video)", "");
            video.Title = video.Title.Replace(" (Official video)", "");
            video.Title = video.Title.Replace(" [OFFICIAL VIDEO]", "");
            video.Title = video.Title.Replace(" [Official Video]", "");
            video.Title = video.Title.Replace(" (OFFICIAL VIDEO)", "");
            video.Title = video.Title.Replace(" (Original Motion Picture Soundtrack)", "");
            video.Title = video.Title.Replace(" [ORIGINAL VIDEO]", "");
            video.Title = video.Title.Replace(" (Original Version)", "");
            video.Title = video.Title.Replace(" (Remix)", "");
            video.Title = video.Title.Replace(" (US Version)", "");
            video.Title = video.Title.Replace(" (Video)", "");
            video.Title = video.Title.Replace(" [Video]", "");
            video.Title = video.Title.Replace(" (Vídeo Oficial)", "");
            video.Title = video.Title.Replace(" (Video Version)", "");
            video.Title = video.Title.Trim();

            string newPath = ImportPath + video.Artist + " - " + video.Title + video.Extension;

            if (newPath != video.Path)
            {
                if (File.Exists(newPath))
                {
                    Log("Cannot move file, destination already exists:");
                    Log("old path : " + video.Path);
                    Log("new path : " + newPath);
                    video.Path = newPath;
                    return false;
                }
                else
                {
                    Log("Renaming File:");
                    Log("Old Path : " + video.Path);
                    Log("New Path : " + newPath);
                    File.Move(video.Path, newPath);
                    video.Path = newPath;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if the video file already exists.
        /// </summary>
        /// <param name="video"></param>
        /// <returns></returns>
        private static bool CheckIfFileExistInCollection(Video video)
        {
            // Check if the file exists.
            if (File.Exists(BasePath + video.Artist + @"\" + video.Title + @"\" + video.Artist + " - " + video.Title + video.Extension))
            {
                Log("File already exists : " + BasePath + video.Artist + @"\" + video.Title + @"\" + video.Artist + " - " + video.Title + video.Extension);
                File.Delete(video.Path);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Move the video file to the Artist\Title folder.
        /// </summary>
        /// <param name="video"></param>
        /// <returns></returns>
        private static bool MoveToFolder(Video video)
        {
            //Create new folder for file.
            if (!Directory.Exists(BasePath + video.Artist))
            {
                _ = Directory.CreateDirectory(BasePath + video.Artist);
            }

            if (!Directory.Exists(BasePath + video.Artist + @"\" + video.Title))
            {
                _ = Directory.CreateDirectory(BasePath + video.Artist + @"\" + video.Title);
            } 

            //Move the file to new folder.
            File.Move(ImportPath + video.Artist + " - " + video.Title + video.Extension, BasePath + video.Artist + @"\" + video.Title + @"\" + video.Artist + " - " + video.Title + video.Extension);
            video.Path = BasePath + video.Artist + @"\" + video.Title + @"\" + video.Artist + " - " + video.Title + video.Extension;

            return true;
        }

        /// <summary>
        /// Get the video details from the Internet.
        /// </summary>
        /// <param name="video"></param>
        /// <returns></returns>
        private static bool GetDetailFromWeb(Video video)
        {
            while (LastWebSearch.AddSeconds(10) > DateTime.Now)
            {
                System.Threading.Thread.Sleep(1000);
            }

            LastWebSearch = DateTime.Now;

            WebClient client = new WebClient();
            client.Headers.Add("user-agent", "mozilla/5.0 (windows nt 10.0; win64; x64) applewebkit/537.36 (khtml, like gecko) chrome/90.0.4430.93 safari/537.36 edg/90.0.818.50");
            string searchterm = GetSearchEngineString(video.Artist + " - " + video.Title, SearchEngine.GoogleAU);

            string htmlString = "";
            string Artist = "";
            string Album = "";
            string Released = "";
            string Genre = "";

            try
            {
                htmlString = client.DownloadString(searchterm);
            }
            catch(Exception ex)
            {
                Log(ex.Message);
                return false;
            }

            if (htmlString.IndexOf(">Artist</") > 0)
            {
                Artist = htmlString.Substring(htmlString.IndexOf(">Artist</"));
                Artist = Artist.Substring(Artist.IndexOf("<a"));
                Artist = Artist.Substring(Artist.IndexOf(">") + 1);
                Artist = Artist.Substring(0, Artist.IndexOf("<"));
            }

            int dist = GetDamerauLevenshteinDistance(Artist, video.Artist);
            if(dist > 300) //<----------------------------------------------------- usually three.
            {
                if(Artist.Trim().Length == 0)
                {

                }
                else
                {
                    Log("searchterm : " + searchterm);
                    Log("LevenshteinDistance fail " + dist + " video artist : " + video.Artist + " web artist : " + Artist);
                    return false;
                }
            }

            if (htmlString.IndexOf(">Album</") > 0)
            {
                Album = htmlString.Substring(htmlString.IndexOf(">Album</"));
                Album = Album.Substring(Album.IndexOf("<a"));
                Album = Album.Substring(Album.IndexOf(">") + 1);
                Album = Album.Substring(0, Album.IndexOf("<"));
            }

            video.Album = Album;

            if (htmlString.IndexOf(">Released</") > 0)
            {
                Released = htmlString.Substring(htmlString.IndexOf(">Released</"));
                Released = Released.Substring(Released.IndexOf(":"));
                Released = Released.Substring(0, Released.IndexOf("</div>") - 6);
                Released = Detag(Released);
            }

            if (Released.Length == 4)
            {
                video.Released = new DateTime(Convert.ToInt32(Released), 1, 1, 0, 0, 0, DateTimeKind.Local);
            }
            else
            {
                try
                {
                    video.Released = DateTime.Parse(Released);
                }
                catch (Exception ex) 
                {
                    Log(ex.Message + " released : " + Released);
                }
            }

            if (htmlString.IndexOf(">Genre</") > 0)
            {
                Genre = htmlString.Substring(htmlString.IndexOf(">Genre</"));
                Genre = Genre.Substring(Genre.IndexOf(":"));
                Genre = Genre.Substring(0, Genre.IndexOf("</div>"));
                Genre = Detag(Genre);
            }

            if (Genre.Length > 10)
            {
                if (Genre.Substring(Genre.Length - 10) == "and   more")
                {
                    Genre = Genre.Substring(0, Genre.Length - 10);
                }
            }

            Genre = Genre.ToLower();
            Genre = Genre.Replace("&amp;", "&");
            Genre = Genre.Replace(";", ",");
            string[] GenreArray = Genre.Split(',');
            foreach (string gen in GenreArray)
            {
                string next = gen.Trim();
                switch (next)
                {
                    case "":
                        break;

                    case "alternative":
                    case "alternative/indie":
                    case "folk":
                        if (!video.Genres.Contains(FileImporter.Genre.Alternative))
                        {
                            video.Genres.Add(FileImporter.Genre.Alternative);
                        }

                        break;

                    case "blues":
                    case "blues rock":
                        if (!video.Genres.Contains(FileImporter.Genre.Blues))
                        {
                            video.Genres.Add(FileImporter.Genre.Blues);
                        }

                        break;

                    case "country":
                    case "country music":
                    case "country rock":
                    case "bluegrass":
                        if (!video.Genres.Contains(FileImporter.Genre.Country))
                        {
                            video.Genres.Add(FileImporter.Genre.Country);
                        }

                        break;

                    case "dance":
                    case "dance music":
                        if (!video.Genres.Contains(FileImporter.Genre.Dance))
                        {
                            video.Genres.Add(FileImporter.Genre.Dance);
                        }

                        break;

                    case "dance/electronic":
                    case "electronic dance music":
                    case "hard trance":
                    case "trance music":
                        if (!video.Genres.Contains(FileImporter.Genre.Dance))
                        {
                            video.Genres.Add(FileImporter.Genre.Dance);
                        }

                        if (!video.Genres.Contains(FileImporter.Genre.Electronic))
                        {
                            video.Genres.Add(FileImporter.Genre.Electronic);
                        }

                        break;

                    case "dance pop":
                        if (!video.Genres.Contains(FileImporter.Genre.Dance))
                        {
                            video.Genres.Add(FileImporter.Genre.Dance);
                        }

                        if (!video.Genres.Contains(FileImporter.Genre.Pop))
                        {
                            video.Genres.Add(FileImporter.Genre.Pop);
                        }

                        break;

                    case "disco":
                    case "italo disco":
                        if (!video.Genres.Contains(FileImporter.Genre.Pop))
                        {
                            video.Genres.Add(FileImporter.Genre.Pop);
                        }

                        break;

                    case "dubstep":
                        if (!video.Genres.Contains(FileImporter.Genre.Dubstep))
                        {
                            video.Genres.Add(FileImporter.Genre.Dubstep);
                        }

                        break;

                    case "easy listening":
                    case "ambient":
                    case "new age":
                        if (!video.Genres.Contains(FileImporter.Genre.EasyListening))
                        {
                            video.Genres.Add(FileImporter.Genre.EasyListening);
                        }

                        break;

                    case "electronica":
                    case "electro":
                        if (!video.Genres.Contains(FileImporter.Genre.Electronic))
                        {
                            video.Genres.Add(FileImporter.Genre.Electronic);
                        }

                        break;

                    case "electro house":
                        if (!video.Genres.Contains(FileImporter.Genre.House))
                        {
                            video.Genres.Add(FileImporter.Genre.House);
                        }

                        if (!video.Genres.Contains(FileImporter.Genre.Electronic))
                        {
                            video.Genres.Add(FileImporter.Genre.Electronic);
                        }

                        break;

                    case "electropop":
                        if (!video.Genres.Contains(FileImporter.Genre.Pop))
                        {
                            video.Genres.Add(FileImporter.Genre.Pop);
                        }

                        if (!video.Genres.Contains(FileImporter.Genre.Electronic))
                        {
                            video.Genres.Add(FileImporter.Genre.Electronic);
                        }

                        break;

                    case "hip-hop/rap":
                    case "hip hop music":
                    case "urban adult contemporary":
                    case "trap":
                    case "trap music":
                    case "latin trap":
                    case "instrumental hip-hop":
                    case "old school hip-hop":
                    case "rapper":
                        if (!video.Genres.Contains(FileImporter.Genre.HipHop))
                        {
                            video.Genres.Add(FileImporter.Genre.HipHop);
                        }

                        break;

                    case "house":
                    case "house music":
                    case "deep house":
                    case "big room house":
                    case "uk garage":
                        if (!video.Genres.Contains(FileImporter.Genre.House))
                        {
                            video.Genres.Add(FileImporter.Genre.House);
                        }

                        break;

                    case "jazz":
                        if (!video.Genres.Contains(FileImporter.Genre.Jazz))
                        {
                            video.Genres.Add(FileImporter.Genre.Jazz);
                        }

                        break;

                    case "metal":
                    case "alt metal":
                    case "hair metal":
                    case "goth/industrial":
                    case "heavy metal":
                    case "classic metal":
                        if (!video.Genres.Contains(FileImporter.Genre.Metal))
                        {
                            video.Genres.Add(FileImporter.Genre.Metal);
                        }

                        break;

                    case "pop":
                    case "pop music":
                    case "dream pop":
                    case "christian/gospel":
                    case "adult contemporary":
                    case "folk-pop":
                    case "a-cappela":
                    case "percussion":
                    case "teen pop":
                    case "new wave":
                    case "new wave/post-punk":
                    case "singer-songwriter":
                    case "acoustic":
                    case "synth-pop":
                        if (!video.Genres.Contains(FileImporter.Genre.Pop))
                        {
                            video.Genres.Add(FileImporter.Genre.Pop);
                        }

                        break;

                    case "punk":
                    case "punk rock":
                        if (!video.Genres.Contains(FileImporter.Genre.Punk))
                        {
                            video.Genres.Add(FileImporter.Genre.Punk);
                        }

                        break;

                    case "reggae":
                    case "dance hall":
                        if (!video.Genres.Contains(FileImporter.Genre.Reggae))
                        {
                            video.Genres.Add(FileImporter.Genre.Reggae);
                        }

                        break;

                    case "rhythm and blues":
                    case "r&b":
                    case "r&b/soul":
                    case "contemporary r&b":
                    case "contemporary soul":
                    case "classic soul":
                    case "funk":
                    case "soul music":
                        if (!video.Genres.Contains(FileImporter.Genre.RhythmAndBlues))
                        {
                            video.Genres.Add(FileImporter.Genre.RhythmAndBlues);
                        }

                        break;

                    case "rock":
                    case "classic rock":
                    case "hard rock":
                    case "pop rock":
                    case "psychedelic rock":
                    case "alternative rock":
                    case "rock and roll":
                    case "soft rock":
                        if (!video.Genres.Contains(FileImporter.Genre.Rock))
                        {
                            video.Genres.Add(FileImporter.Genre.Rock);
                        }

                        break;

                    case "ska":
                    case "ska/rocksteady":
                        if (!video.Genres.Contains(FileImporter.Genre.Ska))
                        {
                            video.Genres.Add(FileImporter.Genre.Ska);
                        }

                        break;

                    case "techno":
                        if (!video.Genres.Contains(FileImporter.Genre.Techno))
                        {
                            video.Genres.Add(FileImporter.Genre.Techno);
                        }

                        break;

                    default:
                        Log("Unknown Genre: " + next);
                        break;
                }
            }

            return true;
        }

        /// <summary>
        /// Removes HTML tags for the input string.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string Detag(string input)
        {
            string output = "";
            bool copy = false;

            for( int i = 0;i < input.Length; i++)
            {
                if (input.Substring(i,1) == "<")
                {
                    copy = false;
                }
                else if (input.Substring(i, 1) == ">")
                {
                    copy = true;
                }
                else
                {
                    if (copy)
                    {
                        output += input.Substring(i, 1);
                    }
                }
            }

            return output.Trim();
        }

        /// <summary>
        /// The Damerau–Levenshtein distance differs from the classical Levenshtein distance by including transpositions among its allowable operations. The classical Levenshtein distance only allows insertion, deletion, and substitution operations.Modifying this distance by including transpositions of adjacent symbols produces a different distance measure, known as the Damerau–Levenshtein distance.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static int GetDamerauLevenshteinDistance(string s, string t)
        {
            var bounds = new { Height = s.Length + 1, Width = t.Length + 1 };

            int[,] matrix = new int[bounds.Height, bounds.Width];

            for (int height = 0; height < bounds.Height; height++)
            {
                matrix[height, 0] = height;
            }

            for (int width = 0; width < bounds.Width; width++)
            {
                matrix[0, width] = width;
            }

            for (int height = 1; height < bounds.Height; height++)
            {
                for (int width = 1; width < bounds.Width; width++)
                {
                    int cost = (s[height - 1] == t[width - 1]) ? 0 : 1;
                    int insertion = matrix[height, width - 1] + 1;
                    int deletion = matrix[height - 1, width] + 1;
                    int substitution = matrix[height - 1, width - 1] + cost;

                    int distance = Math.Min(insertion, Math.Min(deletion, substitution));

                    if (height > 1 && width > 1 && s[height - 1] == t[width - 2] && s[height - 2] == t[width - 1])
                    {
                        distance = Math.Min(distance, matrix[height - 2, width - 2] + cost);
                    }

                    matrix[height, width] = distance;
                }
            }

            return matrix[bounds.Height - 1, bounds.Width - 1];
        }

        /// <summary>
        /// Makes a thumbnail image from the video.
        /// </summary>
        /// <param name="filepath">Path to the file.</param>
        private static void MakeThumbnail(string filepath)
        {
            string eOut = null;
            string closedpath = "\"" + filepath + "\"";
            string newpath = filepath.Substring(0, filepath.LastIndexOf("."));
            newpath = "\"" + newpath + ".png\"";
            string arguments = "-i " + closedpath + " -ss 00:00:02 -frames:v 1 " + newpath;

            ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", "/c " + "ffmpeg" + " " + arguments)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WorkingDirectory = ffmpegpathEx
            };

            using (Process p = new Process())
            {
                p.StartInfo = processInfo;
                p.ErrorDataReceived += new DataReceivedEventHandler((sender, e) => { eOut += e.Data; });
                _ = p.Start();
                _ = p.WaitForExit(120000);
                if (!p.HasExited)
                {
                    Log("MakeThumbnail Failed for " + filepath);
                }
            }
        }

        /// <summary>
        /// Makes the first waveform image.
        /// </summary>
        /// <param name="filepath">Path to the file.</param>
        private static void MakeWaveForm1(string filepath)
        {
            string closedpath = "\"" + filepath + "\"";
            string newpath = filepath.Substring(0, filepath.LastIndexOf("."));
            newpath = "\"" + newpath + "_wave1.png\"";
            string arguments = "-i " + closedpath + " -filter_complex \"compand=gain=3,showwavespic=s=50000x100:colors=#4040ff\" -frames:v 1 " + newpath;
            string eOut = null;
            ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", "/c " + "ffmpeg" + " " + arguments)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WorkingDirectory = ffmpegpathEx
            };

            using (Process p = new Process())
            {
                p.StartInfo = processInfo;
                p.ErrorDataReceived += new DataReceivedEventHandler((sender, e) => { eOut += e.Data; });
                _ = p.Start();
                _ = p.WaitForExit(120000);
                if (!p.HasExited)
                {
                    Log("MakeWaveForm1 Failed for " + filepath);
                }
            }
        }

        /// <summary>
        /// Makes the second waveform image.
        /// </summary>
        /// <param name="filepath">Path to the file.</param>
        private static void MakeWaveForm2(string filepath)
        {
            string eOut = null;
            string closedpath = "\"" + filepath + "\"";
            string newpath = filepath.Substring(0, filepath.LastIndexOf("."));
            newpath = "\"" + newpath + "_wave2.png\"";
            string arguments = "-i " + closedpath + " -filter_complex \"compand=gain=3,showwavespic=s=50000x100:colors=#323232\" -frames:v 1 " + newpath;
            ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", "/c " + "ffmpeg" + " " + arguments)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WorkingDirectory = ffmpegpathEx
            };

            using (Process p = new Process())
            {
                p.StartInfo = processInfo;
                p.ErrorDataReceived += new DataReceivedEventHandler((sender, e) => { eOut += e.Data; });
                _ = p.Start();
                _ = p.WaitForExit(120000);
                if (!p.HasExited)
                {
                    Log("MakeWaveForm2 Failed for " + filepath);
                }
            }
        }

        /// <summary>
        /// Converts the file from WebM to MP4.
        /// </summary>
        /// <param name="filepath">Path to the file.</param>
        private static bool ConvertWebMtoMP4(Video video)
        {
            string closedpath = "\"" + video.Path + "\"";
            string newpath = video.Path.Substring(0, video.Path.LastIndexOf("."));
            string newpathStriped = newpath + ".mp4";
            newpath = "\"" + newpath + ".mp4\"";
            string arguments = "-i " + closedpath + " -vcodec copy -acodec aac " + newpath;

            Log("Converting MKV");
            Log("closed path " + closedpath);
            Log("new path " + newpath);

            ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", "/c " + "ffmpeg" + " " + arguments)
            {
                CreateNoWindow = false,
                UseShellExecute = true,
                RedirectStandardError = false,
                RedirectStandardOutput = false,
                WorkingDirectory = ffmpegpathEx
            };
            Process p = Process.Start(processInfo);
            p.WaitForExit();

            if (File.Exists(newpathStriped))
            {
                FileInfo newFile = new FileInfo(newpathStriped);
                if (newFile.Length > 1000)
                {
                    Log("Mkv Conversion successful : " + newpathStriped);
                    File.Delete(video.Path);
                    video.Path = newpathStriped;
                    return true;
                }
            }
            else
            {
                Log("File does not exist.");
            }

            Log("Mkv Conversion failed : " + newpath);
            return false;
        }

        /// <summary>
        /// Converts the file from MKV to MP4.
        /// </summary>
        /// <param name="filepath">Path to the file.</param>
        private static bool ConvertMKVtoMP4(Video video)
        {
            string closedpath = "\"" + video.Path + "\"";
            string newpath = video.Path.Substring(0, video.Path.LastIndexOf("."));
            string newpathStriped = newpath + ".mp4";
            newpath = "\"" + newpath + ".mp4\"";
            string arguments = "-i " + closedpath + " -vcodec copy -acodec aac " + newpath;

            Log("Converting MKV");
            Log("closedpath " + closedpath);
            Log("newpath " + newpath);

            ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", "/c " + "ffmpeg" + " " + arguments)
            {
                CreateNoWindow = false,
                UseShellExecute = true,
                RedirectStandardError = false,
                RedirectStandardOutput = false,
                WorkingDirectory = ffmpegpathEx
            };
            Process p = Process.Start(processInfo);
            p.WaitForExit();

            if (File.Exists(newpathStriped))
            {
                FileInfo newFile = new FileInfo(newpathStriped);
                if (newFile.Length > 1000)
                {
                    Log("Mkv Conversion successful : " + newpathStriped);
                    File.Delete(video.Path);
                    video.Path = newpathStriped;
                    video.Extension = ".mp4";
                    return true;
                }
            }
            else
            {
                Log("File does not exist.");
            }

            Log("Mkv Conversion failed : " + newpath);
            return false;
        }

        /// <summary>
        /// Converts the file from AVI to MP4.
        /// </summary>
        /// <param name="filepath">Path to the file.</param>
        private static void ConvertAVItoMP4(string filepath)
        {
            string closedpath = "\"" + filepath + "\"";
            string newpath = filepath.Substring(0, filepath.LastIndexOf("."));
            newpath = "\"" + newpath + ".mp4\"";
            string arguments = "-i " + closedpath + " -c:a aac -b:a 128k -c:v libx264 -crf 23 " + newpath;

            Log("Converting AVI");
            Log("closed path " + closedpath);
            Log("new path " + newpath);

            ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", "/c " + "ffmpeg" + " " + arguments)
            {
                CreateNoWindow = false,
                UseShellExecute = true,
                RedirectStandardError = false,
                RedirectStandardOutput = false,
                WorkingDirectory = ffmpegpath
            };
            Process p = Process.Start(processInfo);
            p.WaitForExit();
            File.Delete(filepath);
        }

        /// <summary>
        /// Get a string to be used as to search a search engine.
        /// </summary>
        /// <param name="searchTerm">The term to search for.</param>
        /// <param name="engine">The search engine to use.</param>
        /// <returns></returns>
        public static string GetSearchEngineString(string searchTerm, SearchEngine engine)
        {
            //General replacement.
            string returnValue = searchTerm.Replace("+", "%2B");
            returnValue = returnValue.Replace(" ", "+");
            returnValue = returnValue.Replace("%", "%25");
            returnValue = returnValue.Replace(",", "u%2C");
            returnValue = returnValue.Replace("&", "%26");
            returnValue = returnValue.Replace("!", "%21");
            returnValue = returnValue.Replace("@", "%40");
            returnValue = returnValue.Replace("#", "%23");
            returnValue = returnValue.Replace("$", "%24");
            returnValue = returnValue.Replace("^", "%5E");
            returnValue = returnValue.Replace("(", "%28");
            returnValue = returnValue.Replace(")", "%29");
            returnValue = returnValue.Replace("=", "%3D");
            returnValue = returnValue.Replace("`", "%60");
            returnValue = returnValue.Replace("{", "%7B");
            returnValue = returnValue.Replace("}", "%7D");
            returnValue = returnValue.Replace("[", "%5B");
            returnValue = returnValue.Replace("]", "%5D");
            returnValue = returnValue.Replace("|", "%7C");
            returnValue = returnValue.Replace("\\", "%5C");
            returnValue = returnValue.Replace(":", "%3A");
            returnValue = returnValue.Replace(";", "%3B");
            returnValue = returnValue.Replace("'", "%27");
            returnValue = returnValue.Replace("?", "%3F");
            returnValue = returnValue.Replace("/", "%2F");

            //Engine specific replacement.
            switch (engine)
            {
                case SearchEngine.Bing:
                    returnValue = "https://www.bing.com/search?q=" + returnValue;
                    break;

                case SearchEngine.DuckDuckGo:
                    returnValue = "https://duckduckgo.com/?q=" + returnValue;
                    break;

                case SearchEngine.Google:
                    returnValue = "https://www.google.com/search?q=" + returnValue;
                    break;

                case SearchEngine.GoogleAU:
                    returnValue = "https://www.google.com.au/search?q=" + returnValue;
                    break;

                case SearchEngine.WikiPedia:
                    returnValue = "https://en.wikipedia.org/w/index.php?search=" + returnValue;
                    break;
            }

            return returnValue;
        }
    }

    public enum SearchEngine
    {
        Bing,
        DuckDuckGo,
        Google,
        GoogleAU,
        WikiPedia,
    }
}
