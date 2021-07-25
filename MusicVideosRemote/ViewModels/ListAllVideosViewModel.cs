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
    /// ListAllVideosViewModel class.
    /// </summary>
    public class ListAllVideosViewModel : INotifyPropertyChanged
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
        /// Gets or sets the current ListAllVideosViewModel.
        /// </summary>
        internal static ListAllVideosViewModel Current
        {
            get
            {
                Debug.WriteLine("ListAllVideosViewModel.Current Get");
                if (current is null)
                {
                    current = new ListAllVideosViewModel();
                }

                return current;
            }

            set
            {
                Debug.WriteLine("ListAllVideosViewModel.Current Set");
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
                Debug.WriteLine("ListAllVideosViewModel.Videos Get");
                return videos;
            }

            set
            {
                Debug.WriteLine("ListAllVideosViewModel.Videos Set");
                videos = value;
                OnPropertyChanged("Videos");
            }
        }

        private static ListAllVideosViewModel current;
        private List<Video> videos = new List<Video>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ListAllVideosViewModel"/> class.
        /// </summary>
        public ListAllVideosViewModel()
        {
            Debug.WriteLine("ListAllVideosViewModel.Constructor");

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
        private async Task LoadVideosAsync()
        {
            Debug.WriteLine("ListAllVideosViewModel.LoadVideosAsync");
            try
            {
                DataStore database = await DataStore.Instance;
                videos = await database.GetAllVideosAsync();

                Debug.WriteLine($"Videos found : {videos.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}