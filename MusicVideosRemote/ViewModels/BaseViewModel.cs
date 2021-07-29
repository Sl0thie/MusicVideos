namespace MusicVideosRemote.ViewModels
{
    using MusicVideosRemote.Services;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
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
        public Command NextVideoVideoCommand { get; }

        /// <summary>
        /// Gets the Volume command.
        /// </summary>
        public Command VolumeCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseViewModel"/> class.
        /// </summary>
        public BaseViewModel()
        {
            NextVideoVideoCommand = new Command(CallCommandNextVideo);
        }

        private void CallCommandNextVideo()
        {
            _ = SignalRClient.Current.CommandNextVideo();
        }
    }
}