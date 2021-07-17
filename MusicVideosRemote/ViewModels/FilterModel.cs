namespace MusicVideosRemote.ViewModels
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using MusicVideosRemote.Models;
    using MusicVideosRemote.Services;


    public class FilterModel : INotifyPropertyChanged
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

        private static FilterModel current;

        internal static FilterModel Current
        {
            get
            {
                if (current is null)
                {
                    current = new FilterModel();
                }
                return current;
            }
            set 
            { 
                current = value; 
            }
        }

        private Filter filter;

        public Filter Filter
        {
            get
            {
                if(filter is null)
                {
                    SignalRClient.Current.GetFilterAsync();
                }
                else
                {
                    Debug.WriteLine($"Filter Get: Min {filter.RatingMinimum} Max {filter.RatingMaximum}");
                }               
                return filter;
            }
            set
            {
                Filter lastFilter = filter;
                filter = value;
                if(filter != null)
                {
                    Debug.WriteLine($"Filter Set: Min {filter.RatingMinimum} Max {filter.RatingMaximum}");

                    bool diff = false;
                    if(filter.DateTimeMaximum != lastFilter.DateTimeMaximum) diff = true;
                    if (filter.DateTimeMinimum != lastFilter.DateTimeMinimum) diff = true;
                    //if (filter.Genres != lastFilter.Genres) diff = true;
                    if (filter.RatingMaximum != lastFilter.RatingMaximum) diff = true;
                    if (filter.RatingMinimum != lastFilter.RatingMinimum) diff = true;

                    if (diff)
                    {
                        SignalRClient.Current.SendFilterAsync(filter);
                    }
                }
            }
        }

        public FilterModel()
        {
            Current = this;
        }
    }
}