namespace MusicVideosRemote.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Text;
    using System.Threading.Tasks;
    using MusicVideosRemote.Models;
    using MusicVideosRemote.Services;

    /// <summary>
    /// VideosSearchViewModel class.
    /// </summary>
    public class VideosSearchViewModel : BaseViewModel
    {
        /// <summary>
        /// Gets or sets the current ListAllVideosViewModel.
        /// </summary>
        internal static VideosSearchViewModel Current
        {
            get
            {
                Debug.WriteLine("VideosSearchViewModel.Current Get");

                if (current is null)
                {
                    current = new VideosSearchViewModel();
                }

                return current;
            }

            set
            {
                Debug.WriteLine("VideosSearchViewModel.Current Set");

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

        public string SearchTerm
        {
            get
            {
                return searchTerm;
            }

            set
            {
                searchTerm = value;
                _ = SearchDatabaseAsync(searchTerm);
            }
        }

        private static VideosSearchViewModel current;
        private ObservableCollection<Video> videos;
        private string totalVideos;
        private string searchTerm;

        /// <summary>
        /// Initializes a new instance of the <see cref="VideosSearchViewModel"/> class.
        /// </summary>
        public VideosSearchViewModel()
        {
            Current = this;
        }

        /// <summary>
        /// Search for videos matching the term.
        /// </summary>
        /// <param name="searchTerm">The term to search for.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SearchDatabaseAsync(string searchTerm)
        {
            Debug.WriteLine("VideosSearchViewModel.SearchDatabase " + searchTerm);

            try
            {
                if (searchTerm.Length > 3)
                {
                    DataStore database = await DataStore.Instance;
                    Videos = new ObservableCollection<Video>(await database.GetVideosFromTermAsync(searchTerm));
                    if (videos.Count > 0)
                    {
                        Debug.WriteLine($"No of Videos = {videos.Count}");
                        OnPropertyChanged(string.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}