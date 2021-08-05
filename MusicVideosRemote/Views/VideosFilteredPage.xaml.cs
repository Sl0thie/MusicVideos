namespace MusicVideosRemote.Views
{
    using MusicVideosRemote.ViewModels;
    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;

    /// <summary>
    /// VideosFilteredPage class.
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VideosFilteredPage : ContentPage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VideosFilteredPage"/> class.
        /// </summary>
        public VideosFilteredPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// OnAppearing override for binding.
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();
            BindingContext = new VideosFilteredViewModel();
        }
    }
}