using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

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

                //Output file details.
                // Console.WriteLine("filepath = " + nextpath);
                // Console.WriteLine("filename = " + filename);
                // Console.WriteLine("extension = " + extension);
                // Console.WriteLine("artist = " + artist);
                // Console.WriteLine("title = " + title);
                // Console.WriteLine("");

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

                //Make the thumbnail.
                MakeThumbnail(BasePath + artist + @"\" + title + @"\" + artist + " - " + title + extension);

                //Make the first waveform.
                MakeWaveForm1(BasePath + artist + @"\" + title + @"\" + artist + " - " + title + extension);

                //Make the second waveform.
                MakeWaveForm2(BasePath + artist + @"\" + title + @"\" + artist + " - " + title + extension);

                //Make the json file.
                GetDetails(BasePath + artist + @"\" + title + @"\" + artist + " - " + title + extension, artist, title, extension);

                //Write each time due to file errors.
                string json = JsonConvert.SerializeObject(Videos, Formatting.None);
                File.WriteAllText(BasePath + "index.json", json);

                i++;
                //if (i > 100)
                //{
                //    break;
                //}
            }
        }

        /// <summary>
        /// Obtains the metadata details for the file. 
        /// </summary>
        /// <param name="filepath">Path to the file.</param>
        /// <param name="artist">The aritist's name.</param>
        /// <param name="title">The song's title.</param>
        /// <param name="extension">The extension of the file.</param>
        private static void GetDetails(string filepath, string artist, string title, string extension)
        {
            TimeSpan duration = GetVideoDuration(filepath);
            Video nextVideo = new Video
            {
                Id = ++NoOfVideos,
                Artist = artist,
                Extension = extension,
                Path = filepath,
                Title = title,
                VideoBitRate = 0,
                Duration = (int)duration.TotalMilliseconds,
                VideoWidth = GetVideoWidth(filepath),
                VideoHeight = GetVideoHeight(filepath),
                VideoFPS = GetVideoRate(filepath)
            };
            Videos.Add(nextVideo.Id, nextVideo);
        }

        /// <summary>
        /// Gets the duration of the video.
        /// </summary>
        /// <param name="filePath">Path to the file.</param>
        /// <returns></returns>
        private static TimeSpan GetVideoDuration(string filePath)
        {
            using (var shell = ShellObject.FromParsingName(filePath))
            {
                IShellProperty prop = shell.Properties.System.Media.Duration;
                if (prop.ValueAsObject is object)
                {
                    var t = (ulong)prop.ValueAsObject;
                    return TimeSpan.FromTicks((long)t);
                }
                else
                {
                    return new TimeSpan(0);
                }
            }
        }

        /// <summary>
        /// Gets the width of the video in pixels.
        /// </summary>
        /// <param name="filePath">Path to the file.</param>
        /// <returns></returns>
        private static int GetVideoWidth(string filePath)
        {
            using (var shell = ShellObject.FromParsingName(filePath))
            {
                IShellProperty prop = shell.Properties.System.Video.FrameWidth;
                if (prop.ValueAsObject is object)
                {
                    var t = (uint)prop.ValueAsObject;
                    return (int)t;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Gets the video's height in pixels.
        /// </summary>
        /// <param name="filePath">Path to the file.</param>
        /// <returns></returns>
        private static int GetVideoHeight(string filePath)
        {
            using (var shell = ShellObject.FromParsingName(filePath))
            {
                IShellProperty prop = shell.Properties.System.Video.FrameHeight;
                if (prop.ValueAsObject is object)
                {
                    var t = (uint)prop.ValueAsObject;
                    return (int)t;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Gets the bit rate of the video.
        /// </summary>
        /// <param name="filePath">Path to the file.</param>
        /// <returns></returns>
        private static int GetVideoRate(string filePath)
        {
            using (var shell = ShellObject.FromParsingName(filePath))
            {
                IShellProperty prop = shell.Properties.System.Video.FrameRate;
                if (prop.ValueAsObject is object)
                {
                    var t = (uint)prop.ValueAsObject;
                    return (int)t;
                }
                else
                {
                    return 0;
                }
            }
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
