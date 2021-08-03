namespace MusicVideosRemote.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using MusicVideosRemote.Models;
    using MusicVideosRemote.Services;
    using MusicVideosRemote.Views;

    /// <summary>
    /// VideosFilteredViewModel class.
    /// </summary>
    public class VideosFilteredViewModel : BaseViewModel
    {
        /// <summary>
        /// Gets or sets the current FilteredVideosViewModel.
        /// </summary>
        internal static VideosFilteredViewModel Current
        {
            get
            {
                Debug.WriteLine("VideosFilteredViewModel.Current.Get");

                if (current is null)
                {
                    current = new VideosFilteredViewModel();
                }

                return current;
            }

            set
            {
                Debug.WriteLine("VideosFilteredViewModel.Current.Set");

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
                Debug.WriteLine("VideosFilteredViewModel.Videos.Get");

                return videos;
            }

            set
            {
                Debug.WriteLine("VideosFilteredViewModel.Videos.Set");

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

        private static VideosFilteredViewModel current;
        private ObservableCollection<Video> videos = new ObservableCollection<Video>();
        private string totalVideos;

        private VideosFilteredViewModel()
        {
            Debug.WriteLine("VideosFilteredViewModel.VideosFilteredViewModel");

            Current = this;
            Videos.CollectionChanged += Videos_CollectionChanged;

            _ = LoadVideosAsync();
        }

        private void Videos_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Debug.WriteLine("VideosFilteredViewModel.Videos_CollectionChanged");

            OnPropertyChanged("Videos");
        }

        /// <summary>
        /// Updates a video in the collection.
        /// </summary>
        /// <param name="video">The video to update.</param>
        public void UpdateVideo(Video video)
        {
            Debug.WriteLine("FilteredVideosViewModel.UpdateVideo");

            if (Settings.Current.PassFilter(video))
            {
                if (videos.Any(vid => vid.Id == video.Id))
                {
                    Video existing = videos.First(vid => vid.Id == video.Id);
                    int index = videos.IndexOf(existing);
                    Videos[index] = video;
                }
                else
                {
                    Videos.Add(video);
                }
            }
            else
            {
                if (videos.Any(vid => vid.Id == video.Id))
                {
                    Video existing = videos.First(vid => vid.Id == video.Id);
                    int index = videos.IndexOf(existing);
                    Videos.RemoveAt(index);
                }
            }

            TotalVideos = $"{videos.Count} videos";
        }

        /// <summary>
        /// Method to update the collection when the filter is updated.
        /// </summary>
        public void UpdateFilter()
        {
            _ = LoadVideosAsync();
        }

        /// <summary>
        /// Loads the Videos list with video objects.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task LoadVideosAsync()
        {
            Debug.WriteLine("FilteredVideosViewModel.LoadVideosAsync");

            try
            {
                DataStore database = await DataStore.Instance;
                Videos = new ObservableCollection<Video>(await database.GetFilteredVideosAsync(Settings.Current.Filter));
                VideosFilteredPage.Current.Rebind();
                TotalVideos = $"{videos.Count} videos";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}