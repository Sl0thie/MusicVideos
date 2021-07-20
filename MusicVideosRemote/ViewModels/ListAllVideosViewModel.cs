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
                if (current is null)
                {
                    current = new ListAllVideosViewModel();
                }
                return current;
            }
            set { current = value; }
        }

        //List<Video> videos = new List<Video>();
        private List<Video> videos = new List<Video>();

        public List<Video> Videos
        {
            get { return videos; }
            set 
            { 
                videos = value;
                OnPropertyChanged("Videos");
            }
        }

        public ListAllVideosViewModel()
        {
            Current = this;
            _ = LoadVideosAsync();
        }

        private async Task LoadVideosAsync()
        {
            Debug.WriteLine("LoadVideosAsync");
            DataStore database = await DataStore.Instance;
            videos = await database.GetAllVideosAsync();
        }
    }
}
