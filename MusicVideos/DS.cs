namespace MusicVideos
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Timers;
    using LogCore3;
    using Newtonsoft.Json;

    /// <summary>
    /// DS (DataStrore) class.
    /// </summary>
    public static class DS
    {
        // private static readonly Queue<TimelineItem> TimeLineItems = new Queue<TimelineItem>();
        private static Videos videos;
        private static Settings settings;
        private static Comms comms;
        private static Timer mainTimer;

        /// <summary>
        /// Gets the videos object.
        /// </summary>
        public static Videos Videos
        {
            get { return videos; }
        }

        /// <summary>
        /// Gets the Settings object.
        /// </summary>
        public static Settings Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// Gets the Video Connection object.
        /// </summary>
        public static Comms Comms
        {
            get { return comms; }
        }

        /// <summary>
        /// Gets or sets the Primary timer. Used to change the video at the end.
        /// </summary>
        public static Timer MainTimer { get => mainTimer; set => mainTimer = value; }

        /// <summary>
        /// Initializes the data store.
        /// </summary>
        public static void Initialize()
        {
            if (File.Exists("settings.json"))
            {
                string json = File.ReadAllText("settings.json");
                settings = JsonConvert.DeserializeObject<Settings>(json);
            }
            else
            {
                settings = new Settings
                {
                    Filter = new Filter(),
                };
                SaveSettings();
            }

            comms = new Comms();

            videos = new Videos();

            MainTimer = new Timer();
            MainTimer.Elapsed += MainTimer_Elapsed;
            MainTimer.Interval = 5000;
            MainTimer.Start();
        }

        /// <summary>
        /// Save the settings to file.
        /// </summary>
        public static void SaveSettings()
        {
            string json = JsonConvert.SerializeObject(settings, Formatting.None);
            File.WriteAllText("settings.json", json);
        }

        private static void MainTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Log.Info("Timer Tick");

            MainTimer.Stop();

            if (Comms.IsConnected())
            {
                _ = Videos.PlayNextVideoAsync();
            }
            else
            {
                MainTimer.Interval = 5000;
                comms.CheckConnectionAsync();
                MainTimer.Start();
            }
        }
    }
}