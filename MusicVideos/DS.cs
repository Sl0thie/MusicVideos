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
        private static readonly Queue<TimelineItem> TimeLineItems = new Queue<TimelineItem>();
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

                // TODO Remove these temp values.
                // settings.Filter = new Filter();
                SaveSettings();
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

            TimelineItem nextItem = new TimelineItem
            {
                Timestamp = DateTime.Now.AddSeconds(5),
                ActionItem = () =>
                {
                    comms.CheckConnectionAsync();
                },
            };

            TimeLineItems.Enqueue(nextItem);
        }

        /// <summary>
        /// To be replaced.
        /// </summary>
        /// <param name="newItem">N/A.</param>
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

            if (DS.Comms.IsConnected())
            {
                DS.Videos.PlayNextVideoAsync();
            }
            else
            {
                MainTimer.Interval = 5000;
                MainTimer.Start();
                comms.CheckConnectionAsync();
            }

            //MainTimer.Stop();

            //if (TimeLineItems.Count > 0)
            //{
            //    TimelineItem item = TimeLineItems.Dequeue();

            //    item.ActionItem();

            //    if (TimeLineItems.Count > 0)
            //    {
            //        TimelineItem nextItem = TimeLineItems.Peek();
            //        TimeSpan time = nextItem.Timestamp.Subtract(DateTime.Now);
            //        MainTimer.Interval = time.TotalMilliseconds;
            //        MainTimer.Start();
            //    }
            //    else
            //    {
            //        _ = Videos.PickRandomVideoAsync();
            //        return;
            //    }
            //}
            //else
            //{
            //    _ = Videos.PickRandomVideoAsync();
            //    return;
            //}

            //MainTimer.Start();
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