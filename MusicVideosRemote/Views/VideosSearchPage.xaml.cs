namespace MusicVideosRemote.Views
{
    using System.Diagnostics;
    using MusicVideosRemote.ViewModels;
    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;

    /// <summary>
    /// VideosSearchPage class.
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VideosSearchPage : ContentPage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VideosSearchPage"/> class.
        /// </summary>
        public VideosSearchPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// OnAppearing override for bindings.
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();
            BindingContext = VideosSearchViewModel.Current;
        }

        //private void CV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //}
    }
}