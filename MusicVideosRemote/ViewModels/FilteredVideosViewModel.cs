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
                if (current is null)
                {
                    current = new FilteredVideosViewModel();
                }

                return current;
            }

            set
            {
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
                return videos;
            }

            set
            {
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
                DataStore database = await DataStore.Instance;
                List<Video> allVideos = await database.GetAllVideosAsync();

                Debug.WriteLine($"Videos found : {videos.Count}");

                videos = new List<Video>();

                foreach (Video next in allVideos)
                {
                    if (next.Rating < FilterViewModel.Current.Filter.RatingMaximum)
                    {
                        if (next.Rating > FilterViewModel.Current.Filter.RatingMinimum)
                        {
                            Debug.WriteLine($"FilteredVideosViewModel.LoadVideosAsync adding {next.Artist} - {next.Title}");
                            videos.Add(next);
                        }
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