namespace MusicVideosRemote.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using MusicVideosRemote.Models;
    using MusicVideosRemote.Services;

    /// <summary>
    /// FilteredVideosViewModel class.
    /// </summary>
    public class FilteredVideosViewModel : INotifyPropertyChanged
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
        /// Gets or sets the current FilteredVideosViewModel.
        /// </summary>
        internal static FilteredVideosViewModel Current
        {
            get
            {
                Debug.WriteLine("FilteredVideosViewModel.Current.Get");

                if (current is null)
                {
                    current = new FilteredVideosViewModel();
                }

                return current;
            }

            set
            {
                Debug.WriteLine("FilteredVideosViewModel.Current.Set");

                current = value;
            }
        }

        /// <summary>
        /// Gets or sets the Videos.
        /// </summary>
        public List<Video> Videos
        {
            get
            {
                Debug.WriteLine("FilteredVideosViewModel.Videos.Get");

                return videos;
            }

            set
            {
                Debug.WriteLine("FilteredVideosViewModel.Videos.Set");

                videos = value;
                OnPropertyChanged("Videos");
            }
        }

        private static FilteredVideosViewModel current;
        private List<Video> videos = new List<Video>();

        /// <summary>
        /// Initializes a new instance of the <see cref="FilteredVideosViewModel"/> class.
        /// </summary>
        public FilteredVideosViewModel()
        {
            try
            {
                Debug.WriteLine("FilteredVideosViewModel.FilteredVideosViewModel");

                Current = this;
                _ = LoadVideosAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
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
                List<Video> allVideos = await database.GetAllVideosAsync();

                Debug.WriteLine($"allVideos count : {allVideos.Count}");

                videos = new List<Video>();

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
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}