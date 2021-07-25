﻿namespace MusicVideosRemote.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using MusicVideosRemote.Models;
    using MusicVideosRemote.Services;

    public class FilteredVideosViewModel : INotifyPropertyChanged
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

        private static FilteredVideosViewModel current;

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

        private List<Video> videos = new List<Video>();

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

        public async Task LoadVideosAsync()
        {
            try
            {
                DataStore database = await DataStore.Instance;
                List<Video> allVideos = await database.GetAllVideosAsync();
                videos = new List<Video>();

                foreach (Video next in allVideos)
                {
                    if (next.Rating < FilterViewModel.Current.Filter.RatingMaximum)
                    {
                        if (next.Rating > FilterViewModel.Current.Filter.RatingMinimum)
                        {
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