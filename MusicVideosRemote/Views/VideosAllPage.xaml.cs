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
    /// VideosAllPage class.
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VideosAllPage : ContentPage
    {
        private static VideosAllPage current;

        /// <summary>
        /// Gets or sets the current SignalRClient.
        /// </summary>
        internal static VideosAllPage Current
        {
            get
            {
                Debug.WriteLine("VideosAllPage.Current.Get");

                if (current is null)
                {
                    current = new VideosAllPage();
                }

                return current;
            }

            set
            {
                Debug.WriteLine("VideosAllPage.Current.Set");

                current = value;
            }
        }

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
            Debug.WriteLine("VideosAllPage.OnAppearing");

            base.OnAppearing();

            BindingContext = VideosAllViewModel.Current;
            CV.ItemsSource = VideosAllViewModel.Current.Videos;
        }

        /// <summary>
        /// Rebind controls.
        /// </summary>
        public void Rebind()
        {
            Debug.WriteLine("VideosAllPage.Rebind");

            BindingContext = VideosAllViewModel.Current;
            CV.ItemsSource = VideosAllViewModel.Current.Videos;
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
                Debug.WriteLine("VideosAllPage.CV_SelectionChangedAsync");

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
            await SignalRClient.Current.QueueVideoAsync(id);
        }
    }
}