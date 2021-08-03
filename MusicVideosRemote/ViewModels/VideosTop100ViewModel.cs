namespace MusicVideosRemote.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using MusicVideosRemote.Models;
    using MusicVideosRemote.Services;
    using MusicVideosRemote.Views;

    /// <summary>
    /// VideosTop100ViewModel class.
    /// </summary>
    public class VideosTop100ViewModel : BaseViewModel
    {
        /// <summary>
        /// Gets or sets the current FilteredVideosViewModel.
        /// </summary>
        internal static VideosTop100ViewModel Current
        {
            get
            {
                Debug.WriteLine("VideosTop100ViewModel.Current.Get");

                if (current is null)
                {
                    current = new VideosTop100ViewModel();
                }

                return current;
            }

            set
            {
                Debug.WriteLine("VideosTop100ViewModel.Current.Set");

                current = value;
            }
        }

        /// <summary>
        /// Gets or sets the Videos collection.
        /// </summary>
        public ObservableCollection<Video> Videos
        {
            get
            {
                Debug.WriteLine("VideosTop100ViewModel.Videos.Get");

                return videos;
            }

            set
            {
                Debug.WriteLine("VideosTop100ViewModel.Videos.Set");

                videos = value;
                OnPropertyChanged("Videos");
            }
        }

        /// <summary>
        /// Gets or sets the Total Videos.
        /// </summary>
        public string TotalVideos
        {
            get
            {
                return totalVideos;
            }

            set
            {
                totalVideos = value;
                OnPropertyChanged("TotalVideos");
            }
        }

        private static VideosTop100ViewModel current;
        private ObservableCollection<Video> videos = new ObservableCollection<Video>();
        private string totalVideos;

        private VideosTop100ViewModel()
        {
            Debug.WriteLine("VideosTop100ViewModel.VideosFilteredViewModel");

            Current = this;

            _ = LoadVideosAsync();
        }

        /// <summary>
        /// Load the videos from the database.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task LoadVideosAsync()
        {
            Debug.WriteLine("VideosTop100ViewModel.LoadVideosAsync");

            try
            {
                DataStore database = await DataStore.Instance;
                Videos = new ObservableCollection<Video>(await database.GetTop100VideosAsync());

                VideosTop100Page.Current.Rebind();
                TotalVideos = $"{videos.Count} videos";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}