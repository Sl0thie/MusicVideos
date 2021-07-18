namespace MusicVideosRemote.Views
{
    using MusicVideosRemote.Models;
    using MusicVideosRemote.Services;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VideoListPage : ContentPage
    {
        List<Video> videos = new List<Video>();

        public VideoListPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            DataStore database = await DataStore.Instance;
            videos = await database.GetAllVideosAsync();
            CV.ItemsSource = videos;
        }

        private void CV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Video selected = (Video)e.CurrentSelection[0];
                SignalRClient.Current.QueueVideoAsync(selected.Id);
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Error CV_SelectionChanged: {ex.Message}");
            }
        }
    }
}