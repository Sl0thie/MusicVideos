namespace MusicVideosRemote.ViewModels
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using MusicVideosRemote.Models;
    using MusicVideosRemote.Services;
    using Xamarin.Forms;

    /// <summary>
    /// NowplayingModel class.
    /// </summary>
    public class NowplayingModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Interface

        /// <summary>
        /// Occurs when a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Invoke event when properties change.
        /// </summary>
        /// <param name="propertyName">The property name that changed.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
            {
                return;
            }

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        /// <summary>
        /// Gets or sets the current NowplayingModel.
        /// </summary>
        internal static NowplayingModel Current
        {
            get
            {
                Debug.WriteLine("NowplayingModel.Current.Get");

                if (current is null)
                {
                    current = new NowplayingModel();
                }

                return current;
            }

            set
            {
                Debug.WriteLine("NowplayingModel.Current.Set");

                current = value;
            }
        }

        /// <summary>
        /// Gets or sets the CurrentVideo.
        /// </summary>
        public Video CurrentVideo
        {
            get
            {
                Debug.WriteLine("NowplayingModel.CurrentVideo.Get");

                return currentVideo;
            }

            set
            {
                Debug.WriteLine("NowplayingModel.CurrentVideo.Set");

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

        /// <summary>
        /// Gets or sets the LastPlayed.
        /// </summary>
        public string LastPlayed
        {
            get
            {
                Debug.WriteLine("NowplayingModel.LastPlayed.Get");

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
                Debug.WriteLine("NowplayingModel.LastPlayed.Set");

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the Duration.
        /// </summary>
        public string Duration
        {
            get
            {
                Debug.WriteLine("NowplayingModel.Duration.Get");

                Debug.WriteLine($"NPM Duration: {currentVideo.Duration}");

                if (currentVideo.Duration > 0)
                {
                    TimeSpan ts = TimeSpan.FromMilliseconds(currentVideo.Duration);

                    string rv = ts.ToString(@"m\:ss");

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
                Debug.WriteLine("NowplayingModel.Duration.Set");

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the Released.
        /// </summary>
        public string Released
        {
            get
            {
                Debug.WriteLine("NowplayingModel.Released.Get");

                if (currentVideo.Released == DateTime.MinValue)
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
                Debug.WriteLine("NowplayingModel.Released.Set");

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the Size.
        /// </summary>
        public string Size
        {
            get
            {
                Debug.WriteLine("NowplayingModel.Size.Get");

                if (currentVideo.VideoWidth == 0)
                {
                    return "Unknown";
                }
                else
                {
                    return currentVideo.VideoWidth + " x " + currentVideo.VideoHeight;
                }
            }
        }

        /// <summary>
        /// Gets the Previous Video command.
        /// </summary>
        public Command PreviousVideoCommand { get; }

        /// <summary>
        /// Gets the Play Video command.
        /// </summary>
        public Command PlayVideoCommand { get; }

        /// <summary>
        /// Gets the Pause Video command.
        /// </summary>
        public Command PauseVideoCommand { get; }

        /// <summary>
        /// Gets the Next Video command.
        /// </summary>
        public Command NextVideoVideoCommand { get; }

        /// <summary>
        /// Gets the Volume command.
        /// </summary>
        public Command VolumeCommand { get; }

        private static NowplayingModel current;
        private Video currentVideo;

        /// <summary>
        /// Initializes a new instance of the <see cref="NowplayingModel"/> class.
        /// </summary>
        public NowplayingModel()
        {
            Debug.WriteLine("NowplayingModel.NowplayingModel");

            Current = this;

            NextVideoVideoCommand = new Command(CallCommandNextVideo);
        }

        private void CallCommandNextVideo()
        {
            Debug.WriteLine("NowplayingModel.CallCommandNextVideo");

            _ = SignalRClient.Current.CommandNextVideo();
        }
    }
}