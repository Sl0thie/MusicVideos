namespace MusicVideosRemote.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using MusicVideosRemote.Models;
    using MusicVideosRemote.Services;
    using MusicVideosRemote.Views;

    /// <summary>
    /// VideosAllViewModel class.
    /// </summary>
    public class VideosAllViewModel : BaseViewModel
    {
        /// <summary>
        /// Gets or sets the current ListAllVideosViewModel.
        /// </summary>
        internal static VideosAllViewModel Current
        {
            get
            {
                Debug.WriteLine("VideosAllViewModel.Current Get");

                if (current is null)
                {
                    current = new VideosAllViewModel();
                }

                return current;
            }

            set
            {
                Debug.WriteLine("VideosAllViewModel.Current Set");

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
                Debug.WriteLine("VideosAllViewModel.Videos.Get");

                return videos;
            }

            set
            {
                Debug.WriteLine("VideosAllViewModel.Videos.Set");

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

        private static VideosAllViewModel current;
        private ObservableCollection<Video> videos;
        private string totalVideos;

        /// <summary>
        /// Initializes a new instance of the <see cref="VideosAllViewModel"/> class.
        /// </summary>
        public VideosAllViewModel()
        {
            Debug.WriteLine("VideosAllViewModel.VideosAllViewModel");

            try
            {
                Current = this;
                _ = LoadVideosAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Loads the Videos collection with video objects.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task LoadVideosAsync()
        {
            Debug.WriteLine("VideosAllViewModel.LoadVideosAsync");

            try
            {
                DataStore database = await DataStore.Instance;
                Videos = new ObservableCollection<Video>(await database.GetAllVideosAsync());
                TotalVideos = $"{videos.Count} videos";
                VideosAllPage.Current.Rebind();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}