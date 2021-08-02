namespace MusicVideosRemote.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using MusicVideosRemote.Models;

    /// <summary>
    /// NowPlayingViewModel class.
    /// </summary>
    public class NowPlayingViewModel : BaseViewModel
    {
        /// <summary>
        /// Gets or sets the current NowplayingModel.
        /// </summary>
        internal static NowPlayingViewModel Current
        {
            get
            {
                Debug.WriteLine("NowPlayingViewModel.Current.Get");

                if (current is null)
                {
                    current = new NowPlayingViewModel();
                }

                return current;
            }

            set
            {
                Debug.WriteLine("NowPlayingViewModel.Current.Set");

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
                Debug.WriteLine("NowPlayingViewModel.CurrentVideo.Get");

                return currentVideo;
            }

            set
            {
                Debug.WriteLine("NowPlayingViewModel.CurrentVideo.Set");

                currentVideo = value;
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
                Debug.WriteLine("NowPlayingViewModel.LastPlayed.Get");

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
                Debug.WriteLine("NowPlayingViewModel.LastPlayed.Set");

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
                Debug.WriteLine("NowPlayingViewModel.Duration.Get");

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
                Debug.WriteLine("NowPlayingViewModel.Duration.Set");

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
                Debug.WriteLine("NowPlayingViewModel.Released.Get");

                if (currentVideo.ReleasedYear == 1900)
                {
                    return "Unknown";
                }
                else
                {
                    try
                    {
                        int years = DateTime.Now.Year - currentVideo.ReleasedYear;

                        switch (years)
                        {
                            case 0:
                                return currentVideo.ReleasedYear + " - This Year";

                            case 1:
                                return currentVideo.ReleasedYear + " - Last Year";

                            default:
                                return currentVideo.ReleasedYear + " - " + years + " Years ago";
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
                Debug.WriteLine("NowPlayingViewModel.Released.Set");

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
                Debug.WriteLine("NowPlayingViewModel.Size.Get");

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

        private static NowPlayingViewModel current;
        private Video currentVideo;

        /// <summary>
        /// Initializes a new instance of the <see cref="NowPlayingViewModel"/> class.
        /// </summary>
        public NowPlayingViewModel()
        {
            Debug.WriteLine("NowPlayingViewModel.NowPlayingViewModel");

            Current = this;
        }
    }
}
