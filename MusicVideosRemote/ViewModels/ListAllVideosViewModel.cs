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

    public class ListAllVideosViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private static ListAllVideosViewModel current;

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

        private List<Video> videos = new List<Video>();

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

        public ListAllVideosViewModel()
        {
            Debug.WriteLine("ListAllVideosViewModel.Constructor");

            try
            {
                Current = this;
                _ = LoadVideosAsync();
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async Task LoadVideosAsync()
        {
            Debug.WriteLine("ListAllVideosViewModel.LoadVideosAsync");
            try
            {
                DataStore database = await DataStore.Instance;
                videos = await database.GetAllVideosAsync();

                Debug.WriteLine($"Videos found : { videos.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}