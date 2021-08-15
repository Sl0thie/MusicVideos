namespace MusicVideosRemote.ViewModels
{
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using MusicVideosRemote.Models;
    using MusicVideosRemote.Services;

    /// <summary>
    /// VideosAllViewModel class.
    /// </summary>
    internal class VideosAllViewModel : BaseViewModel
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

        private ObservableCollection<Video> videos;
        private string totalVideos;

        /// <summary>
        /// Initializes a new instance of the <see cref="VideosAllViewModel"/> class.
        /// </summary>
        public VideosAllViewModel()
        {
            _ = LoadVideosAsync();
        }

        /// <summary>
        /// Loads the Videos collection with video objects.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task LoadVideosAsync()
        {
            DataStore database = await DataStore.Instance;
            Videos = new ObservableCollection<Video>(await database.GetAllVideosAsync());
            TotalVideos = $"{videos.Count} videos";
        }
    }
}