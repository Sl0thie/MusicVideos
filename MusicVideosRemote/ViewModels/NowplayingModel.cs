namespace MusicVideosRemote.ViewModels
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using MusicVideosRemote.Models;


    public class NowplayingModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private static NowplayingModel current;

        internal static NowplayingModel Current
        {
            get
            {
                if (current is null)
                {
                    current = new NowplayingModel();
                }
                return current;
            }
            set { current = value; }
        }

        private Video currentVideo;
        public Video CurrentVideo
        {
            get { return currentVideo; }
            set
            {
                currentVideo = value;
                Debug.WriteLine($"NPM Artist: {currentVideo.Artist}");
                Debug.WriteLine($"NPM Title: {currentVideo.Title}");
                OnPropertyChanged("CurrentVideo");
                OnPropertyChanged("LastPlayed");
                OnPropertyChanged("Duration");
                OnPropertyChanged("Released");
                OnPropertyChanged("Size");
            }
        }

        public string LastPlayed
        {
            get
            {
                if (currentVideo.LastPlayed == DateTime.MinValue)
                {
                    return "Never";
                }
                else
                {
                    string rv = currentVideo.LastPlayed.ToString("d/M/yyyy");
                    int days = DateTime.Now.Subtract(currentVideo.LastPlayed).Days;
                    switch (days)
                    {
                        case 0:
                            rv += " - Today";
                            break;

                        case 1:
                            rv += " - Yesterday";
                            break;

                        default:
                            rv += " - " + days + " days ago";
                            break;
                    }
                    return rv;
                }
            }
            set
            {
                OnPropertyChanged();
            }
        }

        public string Duration
        {
            get
            {
                Debug.WriteLine($"NPM Duration: {currentVideo.Duration}");

                if (currentVideo.Duration > 0)
                {
                    TimeSpan ts = TimeSpan.FromMilliseconds(currentVideo.Duration);

                    string rv = ts.ToString();

                    Debug.WriteLine($"NPM Duration String: {rv}");


                    return rv;
                }
                else
                {
                    return "Unknown";
                }
            }
            set
            {
                OnPropertyChanged();
            }
        }

        public string Released
        {
            get
            {
                if(currentVideo.Released == DateTime.MinValue)
                {
                    return "Unknown";
                }
                else
                {
                    try
                    {
                        int days = DateTime.Now.Subtract(currentVideo.Released).Days;
                        int years = days / 365;

                        switch (years)
                        {
                            case 0:
                                return currentVideo.Released.ToString("yyyy") + " - This Year";

                            case 1:
                                return currentVideo.Released.ToString("yyyy") + " - Last Year";

                            default:
                                return currentVideo.Released.ToString("yyyy") + " - " + years + " Years ago";
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"NPM Released Error: {ex.Message}");
                        return "Error";
                    }
                }
            }
            set
            {
                OnPropertyChanged();
            }
        }

        public string Size
        {
            get
            {
                if(currentVideo.VideoWidth == 0)
                {
                    return "Unknown";
                }
                else
                {
                    return currentVideo.VideoWidth + " x " + currentVideo.VideoHeight;
                }
            }
        }

        public NowplayingModel()
        {
            Current = this;
        }
    }
}