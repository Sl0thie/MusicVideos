namespace MusicVideosRemote.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using MusicVideosRemote.Models;
    using MusicVideosRemote.Services;

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

        private static VideosFilteredViewModel current;
        private ObservableCollection<Video> videos;

        private VideosFilteredViewModel()
        {
            Debug.WriteLine("VideosFilteredViewModel.VideosFilteredViewModel");

            Current = this;
        }

        /// <summary>
        /// Loads the Videos list with video objects.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task LoadVideosAsync()
        {
            try
            {
                Debug.WriteLine("FilteredVideosViewModel.LoadVideosAsync");

                DataStore database = await DataStore.Instance;
                List<Video> allVideos = await database.GetFilteredVideosAsync(FilterViewModel.Current.Filter);

                Debug.WriteLine($"allVideos count : {allVideos.Count}");

                videos = new ObservableCollection<Video>();

                Filter filter = FilterViewModel.Current.Filter;

                foreach (Video next in allVideos)
                {
                    if (next.Rating < filter.RatingMaximum)
                    {
                        if (next.Rating > filter.RatingMinimum)
                        {
                            Debug.WriteLine($"FilteredVideosViewModel.LoadVideosAsync adding {next.Artist} - {next.Title} - {next.Rating}");
                            videos.Add(next);
                        }
                        else
                        {
                            // Debug.WriteLine($"FilteredVideosViewModel.LoadVideosAsync skipping {next.Artist} - {next.Title} - {next.Rating}");
                        }
                    }
                    else
                    {
                        // Debug.WriteLine($"FilteredVideosViewModel.LoadVideosAsync skipping {next.Artist} - {next.Title} - {next.Rating}");
                    }
                }

                OnPropertyChanged("Videos");
                OnPropertyChanged(string.Empty);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
