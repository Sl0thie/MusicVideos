namespace MusicVideosRemote.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Filter : INotifyPropertyChanged
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

        public int RatingMinimum
        {
            get
            {
                return ratingMinimum;
            }
            set
            {
                ratingMinimum = value;
                OnPropertyChanged("RatingMinimum");
            }
        }

        public int RatingMaximum
        {
            get
            {
                return ratingMaximum;
            }
            set
            {
                ratingMaximum = value;
                OnPropertyChanged("RatingMaximum");
            }
        }
        public DateTime DateTimeMinimum { get; set; }

        public DateTime DateTimeMaximum { get; set; }


        public List<Genre> Genres = new List<Genre>();
        private int ratingMinimum = 1;
        private int ratingMaximum = 100;
    }
}