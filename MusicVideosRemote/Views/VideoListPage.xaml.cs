namespace MusicVideosRemote.Views
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using MusicVideosRemote.Models;
    using MusicVideosRemote.Services;
    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;

    /// <summary>
    /// VideoListPage class.
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VideoListPage : ContentPage
    {
        private List<Video> videos = new List<Video>();

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoListPage"/> class.
        /// </summary>
        public VideoListPage()
        {
            Debug.WriteLine("VideoListPage.VideoListPage");

            InitializeComponent();
        }

        /// <summary>
        /// OnAppering override to implement binding.
        /// </summary>
        protected override async void OnAppearing()
        {
            Debug.WriteLine("VideoListPage.OnAppearing");

            base.OnAppearing();
            DataStore database = await DataStore.Instance;
            videos = await database.GetAllVideosAsync();
            CV.ItemsSource = videos;
        }

        private void CV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Debug.WriteLine("VideoListPage.CV_SelectionChanged");

                Video selected = (Video)e.CurrentSelection[0];
                _ = SignalRClient.Current.QueueVideoAsync(selected.Id);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error CV_SelectionChanged: {ex.Message}");
            }
        }
    }
}