namespace MusicVideosRemote.Views
{
    using MusicVideosRemote.Models;
    using MusicVideosRemote.Services;
    using System.Collections.Generic;
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

            //listView.ItemsSource = videos;

            CV.ItemsSource = videos;
        }
    }
}