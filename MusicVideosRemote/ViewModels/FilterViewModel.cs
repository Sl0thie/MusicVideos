namespace MusicVideosRemote.ViewModels
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using MusicVideosRemote.Models;
    using MusicVideosRemote.Services;

    public class FilterViewModel : INotifyPropertyChanged
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

        private static FilterViewModel current;

        internal static FilterViewModel Current
        {
            get
            {
                if (current is null)
                {
                    current = new FilterViewModel();
                }

                return current;
            }

            set
            {
                current = value;
            }
        }

        private Filter lastFilter;
        private Filter filter;

        public Filter Filter
        {
            get
            {
                if (filter is null)
                {
                    Debug.WriteLine($"Filter Get: Filter is null.");
                    SignalRClient.Current.GetFilterAsync();
                }

                return filter;
            }

            set
            {
                lastFilter = filter;
                filter = value;
                if (filter != null)
                {
                    if (IsFilterEqual(filter, lastFilter))
                    {
                        Debug.WriteLine($"Filter Set: Equal so not calling property change.");
                    }
                    else
                    {
                        Debug.WriteLine($"Filter Set: Filter changed.");
                        SignalRClient.Current.SendFilterAsync(filter);
                        OnPropertyChanged("Filter");
                        _ = FilteredVideosViewModel.Current.LoadVideosAsync();
                    }
                }
                else
                {
                    Debug.WriteLine($"Filter Set: Filter is null.");
                }
            }
        }

        public int RatingMaximum
        {
            get
            {
                return filter.RatingMaximum;
            }

            set
            {
                if (filter.RatingMaximum != value)
                {
                    filter.RatingMaximum = value;
                    OnPropertyChanged("RatingMaximum");
                }
            }
        }

        public int RatingMinimum
        {
            get
            {
                return filter.RatingMinimum;
            }

            set
            {
                if (filter.RatingMinimum != value)
                {
                    filter.RatingMinimum = value;
                    OnPropertyChanged("RatingMinimum");
                }
            }
        }

        private bool IsFilterEqual(Filter first, Filter second)
        {
            bool diff = false;
            if (first.DateTimeMaximum != second.DateTimeMaximum)
            {
                diff = true;
                Debug.WriteLine($"IsFilterEqual.DateTimeMaximum: first {first.DateTimeMaximum} second {filter.DateTimeMaximum}");
            }

            if (first.DateTimeMinimum != second.DateTimeMinimum)
            {
                diff = true;
                Debug.WriteLine($"IsFilterEqual.DateTimeMinimum: first {first.DateTimeMinimum} second {filter.DateTimeMinimum}");
            }

            if (first.RatingMaximum != second.RatingMaximum)
            {
                diff = true;
                Debug.WriteLine($"IsFilterEqual.RatingMaximum: first {first.RatingMaximum} second {filter.RatingMaximum}");
            }

            if (first.RatingMinimum != second.RatingMinimum)
            {
                diff = true;
                Debug.WriteLine($"IsFilterEqual.RatingMinimum: first {first.RatingMinimum} second {filter.RatingMinimum}");
            }

            if ((first.Genres.Count != 0) & (second.Genres.Count != 0))
            {
                if (first.Genres.Count == second.Genres.Count)
                {
                    for (int i = 0; i < 19; i++)
                    {
                        if (first.Genres.Contains((Genre)i))
                        {
                            if (second.Genres.Contains((Genre)i))
                            {
                                // Debug.WriteLine($"IsFilterEqual.Genre {i}: equal");
                            }
                            else
                            {
                                Debug.WriteLine($"IsFilterEqual.Genre {i}: different");
                                diff = true;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    diff = true;
                }
            }

            if (diff)
            {
                return false;
            }

            return true;
        }

        public FilterViewModel()
        {
            Current = this;
            SignalRClient.Current.GetFilterAsync();
        }
    }
}