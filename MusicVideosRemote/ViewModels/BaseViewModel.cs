namespace MusicVideosRemote.ViewModels
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using MusicVideosRemote.Services;
    using Xamarin.Forms;

    /// <summary>
    /// BaseViewModel class.
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
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
        /// Gets or sets the Volume.
        /// </summary>
        public double Volume
        {
            get
            {
                Debug.WriteLine("BaseViewModel.Volume.Get");

                return Settings.Current.Volume;
            }

            set
            {
                Debug.WriteLine("BaseViewModel.Volume.Set");

                Settings.Current.Volume = (int)value;
                OnPropertyChanged("Volume");
            }
        }

        private bool volumeVisible = false;

        public bool VolumeVisible
        {
            get
            {
                Debug.WriteLine("BaseViewModel.VolumeVisible.Get");

                return volumeVisible;
            }

            set
            {
                Debug.WriteLine("BaseViewModel.VolumeVisible.Set");

                volumeVisible = value;
                OnPropertyChanged("VolumeVisible");
            }
        }

        public Command ShowVolumeCommand { get; }

        /// <summary>
        /// Gets the volume up command.
        /// </summary>
        public Command VolumeUpCommand { get; }

        /// <summary>
        /// Gets the volume down command.
        /// </summary>
        public Command VolumeDownCommand { get; }

        /// <summary>
        /// Gets the Previous Video command.
        /// </summary>
        public Command PreviousVideoCommand { get; }

        /// <summary>
        /// Gets the Play Video command.
        /// </summary>
        public Command PlayVideoCommand { get; }

        /// <summary>
        /// Gets the Pause Video command.
        /// </summary>
        public Command PauseVideoCommand { get; }

        /// <summary>
        /// Gets the Next Video command.
        /// </summary>
        public Command NextVideoCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseViewModel"/> class.
        /// </summary>
        public BaseViewModel()
        {
            PreviousVideoCommand = new Command(CallCommandPreviousVideo);
            NextVideoCommand = new Command(CallCommandNextVideo);
            ShowVolumeCommand = new Command(CallCommandShowVolume);
            VolumeUpCommand = new Command(CallCommandVolumeUp);
            VolumeDownCommand = new Command(CallCommandVolumeDown);
        }

        private void CallCommandPreviousVideo()
        {
            _ = SignalRClient.Current.CommandPreviousVideo();
        }

        private void CallCommandNextVideo()
        {
            _ = SignalRClient.Current.CommandNextVideo();
        }

        private void CallCommandShowVolume()
        {
            if (volumeVisible == true)
            {
                VolumeVisible = false;
            }
            else
            {
                VolumeVisible = true;
            }
        }

        private void CallCommandVolumeUp()
        {
            Debug.WriteLine("BaseViewModel.CallCommandVolumeUp");

            Settings.Current.Volume++;
            OnPropertyChanged("Volume");
        }

        private void CallCommandVolumeDown()
        {
            Debug.WriteLine("BaseViewModel.CallCommandVolumeDown");

            Settings.Current.Volume--;
            OnPropertyChanged("Volume");
        }
    }
}