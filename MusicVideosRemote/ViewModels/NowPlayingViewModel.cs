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
    internal class NowPlayingViewModel : BaseViewModel
    {
        /// <summary>
        /// Gets or sets the current NowplayingModel.
        /// </summary>
        internal static NowPlayingViewModel Current
        {
            get
            {
                if (current is null)
                {
                    current = new NowPlayingViewModel();
                }

                return current;
            }

            set
            {
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
                return currentVideo;
            }

            set
            {
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

        /// <summary>
        /// Gets or sets the Duration.
        /// </summary>
        public string Duration
        {
            get
            {
                if (currentVideo.Duration > 0)
                {
                    TimeSpan ts = TimeSpan.FromMilliseconds(currentVideo.Duration);
                    string rv = ts.ToString(@"m\:ss");
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

        /// <summary>
        /// Gets or sets the Released.
        /// </summary>
        public string Released
        {
            get
            {
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
            Current = this;
        }
    }
}