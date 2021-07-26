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
        /// <summary>
        /// Initializes a new instance of the <see cref="NowPlayingPage"/> class.
        /// </summary>
        public NowPlayingPage()
        {
            Debug.WriteLine("NowPlayingPage.NowPlayingPage");

            InitializeComponent();
        }

        /// <summary>
        /// OnAppearing override to implement binding.
        /// </summary>
        protected override void OnAppearing()
        {
            Debug.WriteLine("ListAllVideosPage.OnAppearing");

            base.OnAppearing();
            BindingContext = NowplayingModel.Current;
        }
    }
}