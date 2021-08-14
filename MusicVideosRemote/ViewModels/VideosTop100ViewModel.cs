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
        /// Gets or sets the Videos collection.
        /// </summary>
        public ObservableCollection<Video> Videos
        {
            get
            {
                return videos;
            }

            set
            {
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

        private ObservableCollection<Video> videos = new ObservableCollection<Video>();
        private string totalVideos;

        /// <summary>
        /// Initializes a new instance of the <see cref="VideosTop100ViewModel"/> class.
        /// </summary>
        public VideosTop100ViewModel()
        {
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
                TotalVideos = $"{videos.Count} videos";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}