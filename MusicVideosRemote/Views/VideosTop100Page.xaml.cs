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
    /// VideosTop100Page class.
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VideosTop100Page : ContentPage
    {
        private static VideosTop100Page current;

        /// <summary>
        /// Gets or sets the current SignalRClient.
        /// </summary>
        internal static VideosTop100Page Current
        {
            get
            {
                Debug.WriteLine("VideosFilteredPage.Current.Get");

                if (current is null)
                {
                    current = new VideosTop100Page();
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
        /// Initializes a new instance of the <see cref="VideosTop100Page"/> class.
        /// </summary>
        public VideosTop100Page()
        {
            Debug.WriteLine("VideosTop100Page.VideosTop100Page");

            InitializeComponent();
            current = this;
        }

        /// <summary>
        /// Method to rebind.
        /// </summary>
        public void Rebind()
        {
            Debug.WriteLine("VideosTop100Page.Rebind");

            BindingContext = VideosTop100ViewModel.Current;
            CV.ItemsSource = VideosTop100ViewModel.Current.Videos;
        }

        /// <summary>
        /// OnAppearing override for binding.
        /// </summary>
        protected override void OnAppearing()
        {
            Debug.WriteLine("VideosTop100Page.OnAppearing");

            base.OnAppearing();

            _ = VideosTop100ViewModel.Current.LoadVideosAsync();
            BindingContext = VideosTop100ViewModel.Current;
            CV.ItemsSource = VideosTop100ViewModel.Current.Videos;
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