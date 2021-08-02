namespace MusicVideosRemote.Views
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using MusicVideosRemote.Models;
    using MusicVideosRemote.Services;
    using MusicVideosRemote.ViewModels;
    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;

    /// <summary>
    /// VideosFilteredPage class.
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VideosFilteredPage : ContentPage
    {
        private static VideosFilteredPage current;

        /// <summary>
        /// Gets or sets the current SignalRClient.
        /// </summary>
        internal static VideosFilteredPage Current
        {
            get
            {
                Debug.WriteLine("VideosFilteredPage.Current.Get");

                if (current is null)
                {
                    current = new VideosFilteredPage();
                }

                return current;
            }

            set
            {
                Debug.WriteLine("VideosFilteredPage.Current.Set");

                current = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VideosFilteredPage"/> class.
        /// </summary>
        public VideosFilteredPage()
        {
            Debug.WriteLine("VideosFilteredPage.VideosFilteredPage");

            current = this;
            InitializeComponent();
        }

        /// <summary>
        /// OnAppearing override for binding.
        /// </summary>
        protected override void OnAppearing()
        {
            Debug.WriteLine("VideosFilteredPage.OnAppearing");

            base.OnAppearing();
            BindingContext = VideosFilteredViewModel.Current;
            CV.ItemsSource = VideosFilteredViewModel.Current.Videos;
        }

        /// <summary>
        /// Method to rebind.
        /// </summary>
        public void Rebind()
        {
            Debug.WriteLine("VideosFilteredPage.Rebind");

            BindingContext = VideosFilteredViewModel.Current;
            CV.ItemsSource = VideosFilteredViewModel.Current.Videos;
        }

        /// <summary>
        /// Manages the selection changed event for the CV list.
        /// </summary>
        /// <param name="sender">Unused.</param>
        /// <param name="e">Event arguments from the CV List.</param>
        private void CV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Debug.WriteLine("VideosFilteredPage.CV_SelectionChangedAsync");

                Video selected = (Video)e.CurrentSelection[0];
                _ = ProcessselectedAsync(selected.Id);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error CV_SelectionChanged: {ex.Message}");
            }
        }

        /// <summary>
        /// Processes the item.
        /// </summary>
        /// <param name="id">The id of the video to process.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task ProcessselectedAsync(int id)
        {
            Debug.WriteLine("VideosFilteredPage.ProcessselectedAsync");

            await SignalRClient.Current.QueueVideoAsync(id);
        }
    }
}