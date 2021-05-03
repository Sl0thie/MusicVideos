using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;

[assembly: CLSCompliant(true)]
namespace FileImporter
{
    class Program
    {
        private const string ImportPath = @"E:\New Music Videos\";                     //Path that the files are to be imported from.
        private const string BasePath = @"E:\More Music Videos\";                      //Base path that the files are stored.
        private const string ffmpegpath = "\"C:\\Program Files\\ffmpeg\\bin\"";        //Path to ffmpeg. Enclosed in quotation marks to suit shellex.
        private const string ffmpegpathEx = @"C:\Program Files\ffmpeg\bin";            //Path to ffmpeg. Not enclosed to suit shell.
        private static Dictionary<int, Video> Videos = new Dictionary<int, Video>();   //Dictionary of Video objects related to the files.
        private static int NoOfVideos { get; set; }                                    //No of Videos that are in the store.

        /// <summary>
        /// FilImporter preforms several tasks related to the management of a file collection of music videos. Files are first converted to formats that can be played by HTML5 Video tag. Files are then stored in a "Artist\Title" format after several properties are obtained. Then the details of these operations are recorded in a json file in the base folder.
        /// </summary>
        static void Main()
        {
            ProcessNewVideos();
            Console.Write("Press any key to close this window...");
            Console.ReadKey();
        }

        /// <summary>
        /// Processes files from the import path. The process is additive, it can fail on a file and be restarted again after the file is fixed.
        /// </summary>
        private static void ProcessNewVideos()
        {
            //Get the existing Video data from the index file.
            if (File.Exists(BasePath + "index.json"))
            {
                string fileContent3 = File.ReadAllText(BasePath + "index.json");
                Videos = JsonConvert.DeserializeObject<Dictionary<int, Video>>(fileContent3);
                NoOfVideos = Videos.Count;
                Console.WriteLine("No of Videos : " + NoOfVideos);
            }

            int i = 0; //temp. provides early exit from foreach for testing.

            //Loop through the files in the import path.
            foreach (var nextpath in Directory.EnumerateFiles(ImportPath, "*.*", SearchOption.TopDirectoryOnly))
            {
                //Get file properties.
                string filename = nextpath.Substring(nextpath.LastIndexOf(@"\") + 1);
                string extension = filename.Substring(filename.LastIndexOf(".")).ToLower();
                filename = filename.Substring(0, filename.LastIndexOf("."));
                if (filename.IndexOf(" - ") < 0)
                {
                    Console.WriteLine("File Rejected (name pattern) : " + nextpath);
                    continue;
                }
                string artist = filename.Substring(0, filename.IndexOf(" - ")).Trim();
                string title = filename.Substring(filename.IndexOf(" - ") + 3).Trim();

                // Check for youtube id.
                if(title.Length > 12)
                {
                    if (title.Substring(title.Length - 12, 1) == "-")
                    {
                        if (title.Substring(title.Length - 1, 1) != " ")
                        {
                            Console.WriteLine("Title before : " + title);
                            title = title.Substring(0, title.Length - 12);
                            Console.WriteLine("Title after  : " + title);
                        }
                    }
                }

                // Remove unwanted brackets.
                title = title.Replace(" (AOL Sessions)", "");
                title = title.Replace(" (Cover)", "");
                title = title.Replace(" (Explicit)", "");
                title = title.Replace(" (Japanese Edition)", "");
                title = title.Replace(" (MTV Unplugged)", "");
                title = title.Replace(" (Music Video)", "");
                title = title.Replace(" [MUSIC VIDEO]", "");
                title = title.Replace(" (NEW SONG 2017)", "");
                title = title.Replace(" (New Song 2017)", "");
                title = title.Replace(" (Official)", "");
                title = title.Replace(" [Official]", "");
                title = title.Replace(" [OFFICIAL MUSIC VIDEO]", "");
                title = title.Replace(" Official Music Video", "");
                title = title.Replace(" (Official Video)", "");
                title = title.Replace(" (Official video)", "");
                title = title.Replace(" [OFFICIAL VIDEO]", "");
                title = title.Replace(" [Official Video]", "");
                title = title.Replace(" (OFFICIAL VIDEO)", "");
                title = title.Replace(" (Official Music Video)", "");
                title = title.Replace(" [Official Music Video]", "");
                title = title.Replace(" (OFFICIAL MUSIC VIDEO)", "");
                title = title.Replace(" (Official Audio)", "");
                title = title.Replace(" [Official Audio]", "");
                title = title.Replace(" (Original Motion Picture Soundtrack)", "");
                title = title.Replace(" [ORIGINAL VIDEO]", "");
                title = title.Replace(" (Original Version)", "");
                title = title.Replace(" (Remix)", "");
                title = title.Replace(" (US Version)", "");
                title = title.Replace(" (Video)", "");
                title = title.Replace(" (Vídeo Oficial)", "");
                title = title.Replace(" (Video Version)", "");

                //Create new folder for file.
                if (!Directory.Exists(BasePath + artist))
                {
                    Directory.CreateDirectory(BasePath + artist);
                }
                if (!Directory.Exists(BasePath + artist + @"\" + title))
                {
                    Directory.CreateDirectory(BasePath + artist + @"\" + title);
                }

                //If the file exists already then don't add it again.
                if (File.Exists(BasePath + artist + @"\" + title + @"\" + artist + " - " + title + extension))
                {
                    continue;
                }
                else
                {
                    //Copy the file to new folder.
                    File.Copy(nextpath, BasePath + artist + @"\" + title + @"\" + artist + " - " + title + extension);
                }

                //Get details from the internet.
                GetWebDetails(BasePath + artist + @"\" + title + @"\" + artist + " - " + title + extension,artist, title, extension);

                //Convert avi files.
                if (extension == ".avi")
                {
                    ConvertAVItoMP4(BasePath + artist + @"\" + title + @"\" + artist + " - " + title + extension);
                    extension = ".mp4";
                }

                //Convert mp4 files.
                if (extension == ".mkv")
                {
                    ConvertMKVtoMP4(BasePath + artist + @"\" + title + @"\" + artist + " - " + title + extension);
                    extension = ".mp4";
                }

                ////Make the thumbnail.
                //MakeThumbnail(BasePath + artist + @"\" + title + @"\" + artist + " - " + title + extension);

                ////Make the first waveform.
                //MakeWaveForm1(BasePath + artist + @"\" + title + @"\" + artist + " - " + title + extension);

                ////Make the second waveform.
                //MakeWaveForm2(BasePath + artist + @"\" + title + @"\" + artist + " - " + title + extension);

                ////Make the json file.
                //GetDetails(BasePath + artist + @"\" + title + @"\" + artist + " - " + title + extension, artist, title, extension);

                //Write each time due to file errors.
                string json = JsonConvert.SerializeObject(Videos, Formatting.None);
                File.WriteAllText(BasePath + "index.json", json);

                i++;
                if (i > 100)
                {
                    break;
                }
            }
        }

        private static void GetWebDetails(string filepath, string artist, string title, string extension)
        {
            //https://www.bing.com/search?q=acdc+-+back+in+black&form=ANNTH1&refig=d640f4cdf1b74a6b9328c9bdb3c6ad1a
            WebClient client = new WebClient();
            client.Headers.Add("user-agent", "mozilla/5.0 (windows nt 10.0; win64; x64) applewebkit/537.36 (khtml, like gecko) chrome/90.0.4430.93 safari/537.36 edg/90.0.818.51");
            string searchterm = "https://www.google.com.au/search?q=" + artist.Replace(" ", "+") + "+-+" + title.Replace(" ", "+");
            string htmlString = client.DownloadString(searchterm);

            //Output file details.
            Console.WriteLine("filepath = " + filepath);
            Console.WriteLine("extension = " + extension);
            Console.WriteLine("artist = " + artist);
            Console.WriteLine("title = " + title);
            Console.WriteLine("SearchTerm: " + searchterm);

            string Artist = "";
            if (htmlString.IndexOf(">Artist</") > 0)
            {
                Artist = htmlString.Substring(htmlString.IndexOf(">Artist</"));
                Artist = Artist.Substring(Artist.IndexOf("<a"));
                Artist = Artist.Substring(Artist.IndexOf(">") + 1);
                Artist = Artist.Substring(0,Artist.IndexOf("<"));
            }
            Console.WriteLine("Artist : " + Artist);

            string Album = "";
            if (htmlString.IndexOf(">Album</") > 0)
            {
                Album = htmlString.Substring(htmlString.IndexOf(">Album</"));
                Album = Album.Substring(Album.IndexOf("<a"));
                Album = Album.Substring(Album.IndexOf(">") + 1);
                Album = Album.Substring(0, Album.IndexOf("<"));
            }
            Console.WriteLine("Album : " + Album);

            string Released = "";
            if (htmlString.IndexOf(">Released</") > 0)
            {
                Released = htmlString.Substring(htmlString.IndexOf(">Released</"));
                Released = Released.Substring(Released.IndexOf(":"));
                Released = Released.Substring(0, Released.IndexOf("</div>") - 6);
                Released = Detag(Released);
            }
            Console.WriteLine("Released : " + Released);

            string Genre = "";
            if (htmlString.IndexOf(">Genre</") > 0)
            {
                Genre = htmlString.Substring(htmlString.IndexOf(">Genre</"));
                Genre = Genre.Substring(Genre.IndexOf(":"));
                Genre = Genre.Substring(0,Genre.IndexOf("</div>"));
                Genre = Detag(Genre);
            }
            Console.WriteLine("Genre : " + Genre );
            //Debug.WriteLine("Genre : " + Genre);
            Console.WriteLine("");

            Video nextVideo = new Video
            {
                Id = ++NoOfVideos,
                Artist = artist,
                SearchArtist = artist.Replace("The ", string.Empty),
                Extension = extension,
                Duration = 0,
                Path = filepath,
                Title = title,
                VideoBitRate = 0,
                //Genres = GenreList,
                VideoWidth = 0,
                VideoHeight = 0,
                VideoFPS = 0,
                PlayCount = 0,
                QueuedCount = 0,
                Rating = 50
            };

            if(Released.Length == 4)
            {
                nextVideo.Released = DateTime.Parse("1/1/" + Released);
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
            Genre = Genre.Replace(";",",");
            Collection<Genre> GenreList = new Collection<Genre>();
            string[] GenreArray = Genre.Split(',');
            foreach (string gen in GenreArray)
            {
                string next = gen.Trim();
                switch (next)
                {
                    case "":
                        break;
                    case "alternative/indie":
                        if (!nextVideo.Genres.Contains(FileImporter.Genre.Alternative))
                        {
                            nextVideo.Genres.Add(FileImporter.Genre.Alternative);
                        }
                        break;

                    case "dance/electronic":
                    case "electronic dance music":
                        if (!nextVideo.Genres.Contains(FileImporter.Genre.Dance))
                        {
                            nextVideo.Genres.Add(FileImporter.Genre.Dance);
                        }
                        if (!nextVideo.Genres.Contains(FileImporter.Genre.Electronic))
                        {
                            nextVideo.Genres.Add(FileImporter.Genre.Electronic);
                        }
                        break;

                    case "disco":
                        if (!nextVideo.Genres.Contains(FileImporter.Genre.Pop))
                        {
                            nextVideo.Genres.Add(FileImporter.Genre.Pop);
                        }
                        break;

                    case "easy listening":
                    case "ambient":
                        if (!nextVideo.Genres.Contains(FileImporter.Genre.EasyListening))
                        {
                            nextVideo.Genres.Add(FileImporter.Genre.EasyListening);
                        }
                        break;

                    case "electronica":
                        if (!nextVideo.Genres.Contains(FileImporter.Genre.Electronic))
                        {
                            nextVideo.Genres.Add(FileImporter.Genre.Electronic);
                        }
                        break;

                    case "electro house":
                        if (!nextVideo.Genres.Contains(FileImporter.Genre.House))
                        {
                            nextVideo.Genres.Add(FileImporter.Genre.House);
                        }
                        if (!nextVideo.Genres.Contains(FileImporter.Genre.Electronic))
                        {
                            nextVideo.Genres.Add(FileImporter.Genre.Electronic);
                        }
                        break;

                    case "hip-hop/rap":
                    case "hip hop music":
                        if (!nextVideo.Genres.Contains(FileImporter.Genre.HipHop))
                        {
                            nextVideo.Genres.Add(FileImporter.Genre.HipHop);
                        }
                        break;

                    case "house":
                    case "house music":
                    case "deep house":
                        if (!nextVideo.Genres.Contains(FileImporter.Genre.House))
                        {
                            nextVideo.Genres.Add(FileImporter.Genre.House);
                        }
                        break;

                    case "metal":
                    case "alt metal":
                        if (!nextVideo.Genres.Contains(FileImporter.Genre.Metal))
                        {
                            nextVideo.Genres.Add(FileImporter.Genre.Metal);
                        }
                        break;

                    case "pop":
                    case "pop music":
                    case "dream pop":
                        if (!nextVideo.Genres.Contains(FileImporter.Genre.Pop))
                        {
                            nextVideo.Genres.Add(FileImporter.Genre.Pop);
                        }
                        break;

                    case "rhythm and blues":
                    case "r&b/soul":
                    case "contemporary r&b":
                    case "contemporary soul":
                        if (!nextVideo.Genres.Contains(FileImporter.Genre.RhythmAndBlues))
                        {
                            nextVideo.Genres.Add(FileImporter.Genre.RhythmAndBlues);
                        }
                        break;

                    case "rock":
                    case "classic rock":
                    case "hard rock":
                        if (!nextVideo.Genres.Contains(FileImporter.Genre.Rock))
                        {
                            nextVideo.Genres.Add(FileImporter.Genre.Rock);
                        }
                        break;

                    default:
                        Debug.WriteLine("Unknown Genre: " + next);
                        break;
                }
            }
            
            Videos.Add(nextVideo.Id, nextVideo);
        }

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

            var processInfo = new ProcessStartInfo("cmd.exe", "/c " + "ffmpeg" + " " + arguments)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WorkingDirectory = ffmpegpathEx
            };

            using (var p = new Process())
            {
                p.StartInfo = processInfo;
                p.ErrorDataReceived += new DataReceivedEventHandler((sender, e) => { eOut += e.Data; });
                p.Start();
                p.WaitForExit(120000);
                if (!p.HasExited)
                {
                    Console.WriteLine("MakeThumbnail Failed for " + filepath);
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
            var processInfo = new ProcessStartInfo("cmd.exe", "/c " + "ffmpeg" + " " + arguments)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WorkingDirectory = ffmpegpathEx
            };

            using (var p = new Process())
            {
                p.StartInfo = processInfo;
                p.ErrorDataReceived += new DataReceivedEventHandler((sender, e) => { eOut += e.Data; });
                p.Start();
                p.WaitForExit(120000);
                if (!p.HasExited)
                {
                    Console.WriteLine("MakeWaveForm1 Failed for " + filepath);
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
            var processInfo = new ProcessStartInfo("cmd.exe", "/c " + "ffmpeg" + " " + arguments)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WorkingDirectory = ffmpegpathEx
            };

            using (var p = new Process())
            {
                p.StartInfo = processInfo;
                p.ErrorDataReceived += new DataReceivedEventHandler((sender, e) => { eOut += e.Data; });
                p.Start();
                p.WaitForExit(120000);
                if (!p.HasExited)
                {
                    Console.WriteLine("MakeWaveForm2 Failed for " + filepath);
                }
            }
        }

        /// <summary>
        /// Converts the file from MKV to MP4.
        /// </summary>
        /// <param name="filepath">Path to the file.</param>
        private static void ConvertMKVtoMP4(string filepath)
        {
            string closedpath = "\"" + filepath + "\"";
            string newpath = filepath.Substring(0, filepath.LastIndexOf("."));
            newpath = "\"" + newpath + ".mp4\"";
            string arguments = "-i " + closedpath + " -vcodec copy -acodec aac " + newpath;
            var processInfo = new ProcessStartInfo("cmd.exe", "/c " + "ffmpeg" + " " + arguments)
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
        /// Converts the file from AVI to MP4.
        /// </summary>
        /// <param name="filepath">Path to the file.</param>
        private static void ConvertAVItoMP4(string filepath)
        {
            string closedpath = "\"" + filepath + "\"";
            string newpath = filepath.Substring(0, filepath.LastIndexOf("."));
            newpath = "\"" + newpath + ".mp4\"";
            string arguments = "-i " + closedpath + " -c:a aac -b:a 128k -c:v libx264 -crf 23 " + newpath;
            var processInfo = new ProcessStartInfo("cmd.exe", "/c " + "ffmpeg" + " " + arguments)
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
    }
}
