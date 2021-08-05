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
    }
}