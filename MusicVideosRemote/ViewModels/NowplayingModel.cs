namespace MusicVideosRemote.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;
    using MusicVideosRemote.Models;
    using MusicVideosRemote.Services;

    public class NowplayingModel : INotifyPropertyChanged
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

        //internal static NowplayingModel Current;
        private static NowplayingModel current;

        internal static NowplayingModel Current
        {
            get
            {
                if (current is null)
                {
                    current = new NowplayingModel();
                }
                return current;
            }
            set { current = value; }
        }

        private Video currentVideo;
        public Video CurrentVideo
        {
            get { return currentVideo; }
            set
            {
                currentVideo = value;
                Debug.WriteLine($"NPM Artist: {currentVideo.Artist}");
                Debug.WriteLine($"NPM Title: {currentVideo.Title}");
                OnPropertyChanged();
            }
        }

        public NowplayingModel()
        {
            Current = this;
        }
    }
}