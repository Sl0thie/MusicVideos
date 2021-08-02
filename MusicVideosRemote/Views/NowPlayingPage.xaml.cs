namespace MusicVideosRemote.Views
{
    using System.Diagnostics;
    using MusicVideosRemote.ViewModels;
    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;

    /// <summary>
    /// NowPlayingPage class.
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NowPlayingPage : ContentPage
    {
        private static NowPlayingPage current;

        /// <summary>
        /// Gets or sets the current SignalRClient.
        /// </summary>
        internal static NowPlayingPage Current
        {
            get
            {
                Debug.WriteLine("NowPlayingPage.Current.Get");

                if (current is null)
                {
                    current = new NowPlayingPage();
                }

                return current;
            }

            set
            {
                Debug.WriteLine("NowPlayingPage.Current.Set");

                current = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NowPlayingPage"/> class.
        /// </summary>
        public NowPlayingPage()
        {
            Debug.WriteLine("NowPlayingPage.NowPlayingPage");

            InitializeComponent();

            current = this;
        }

        /// <summary>
        /// OnAppearing override to implement binding.
        /// </summary>
        protected override void OnAppearing()
        {
            Debug.WriteLine("NowPlayingPage.OnAppearing");

            base.OnAppearing();
            BindingContext = NowPlayingViewModel.Current;
        }

        private void ButtonVolume_Clicked(object sender, System.EventArgs e)
        {
            Debug.WriteLine("NowPlayingPage.ButtonVolume_Clicked");

            VolumePopup.IsVisible = true;
        }

        private void Frame_Unfocused(object sender, FocusEventArgs e)
        {
            Debug.WriteLine("NowPlayingPage.Frame_Unfocused");
        }

        private void VolumePopup_Unfocused(object sender, FocusEventArgs e)
        {
            Debug.WriteLine("NowPlayingPage.VolumePopup_Unfocused");
        }

        private void VolumePopup_Focused(object sender, FocusEventArgs e)
        {
            Debug.WriteLine("NowPlayingPage.VolumePopup_Focused");
        }

        private void TapGestureRecognizer_Tapped(object sender, System.EventArgs e)
        {
            Debug.WriteLine("NowPlayingPage.TapGestureRecognizer_Tapped");

            VolumePopup.IsVisible = false;
        }
    }
}