namespace MusicVideos
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Timers;
    using Newtonsoft.Json;

    /// <summary>
    /// DS (DataStrore) class.
    /// </summary>
    public static class DS
    {
        private static Videos videos;
        private static Settings settings;
        private static Comms comms;

        private static Queue<TimelineItem> TimeLineItems = new Queue<TimelineItem>();

        public static System.Timers.Timer MainTimer;

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
        /// Initializes the data store.
        /// </summary>
        public static void Initialize()
        {
            if (File.Exists("settings.json"))
            {
                string json = File.ReadAllText("settings.json");
                settings = JsonConvert.DeserializeObject<Settings>(json);

                // TODO Remove these temp values.
                settings.Filter = new Filter();
                SaveSettings();
            }
            else
            {
                settings = new Settings();
                settings.Filter = new Filter();
            }

            comms = new Comms();

            videos = new Videos();

            MainTimer = new Timer();
            MainTimer.Elapsed += MainTimer_Elapsed;
            MainTimer.Interval = 5000;
            MainTimer.Start();

            TimelineItem nextItem = new TimelineItem();
            nextItem.Timestamp = DateTime.Now.AddSeconds(5);
            nextItem.ActionItem = () =>
            {
                comms.CheckConnectionAsync();
            };

            TimeLineItems.Enqueue(nextItem);
        }

        public static void AddTimelineItem(TimelineItem newItem)
        {
            if (TimeLineItems.Count > 0)
            {
                TimeLineItems.Enqueue(newItem);
            }
            else
            {
                MainTimer.Enabled = false;
                TimeLineItems.Enqueue(newItem);
                TimelineItem nextItem = TimeLineItems.Peek();
                TimeSpan time = nextItem.Timestamp.Subtract(DateTime.Now);
                MainTimer.Interval = time.TotalMilliseconds;
                MainTimer.Enabled = true;
            }
        }

        private static void MainTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Debug.WriteLine("Timer Tick");

            MainTimer.Stop();

            if (TimeLineItems.Count > 0)
            {
                TimelineItem item = TimeLineItems.Dequeue();

                item.ActionItem();

                if (TimeLineItems.Count > 0)
                {
                    TimelineItem nextItem = TimeLineItems.Peek();
                    TimeSpan time = nextItem.Timestamp.Subtract(DateTime.Now);
                    MainTimer.Interval = time.TotalMilliseconds;
                    MainTimer.Start();
                }
                else
                {
                    _ = Videos.PickRandomVideoAsync();
                    return;
                }
            }
            else
            {
                _ = Videos.PickRandomVideoAsync();
                return;
            }

            MainTimer.Start();
        }

        /// <summary>
        /// Save the settings to file.
        /// </summary>
        private static void SaveSettings()
        {
            string json = JsonConvert.SerializeObject(settings, Formatting.None);
            File.WriteAllText("settings.json", json);
        }
    }
}