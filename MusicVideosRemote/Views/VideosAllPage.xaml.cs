namespace MusicVideosRemote.Views
{
    using MusicVideosRemote.ViewModels;
    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;

    /// <summary>
    /// VideosAllPage class.
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VideosAllPage : ContentPage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VideosAllPage"/> class.
        /// </summary>
        public VideosAllPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// OnAppearing override for bindings.
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();
            BindingContext = new VideosAllViewModel();
        }
    }
}