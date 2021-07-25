namespace MusicVideosRemote.Views
{
    using MusicVideosRemote.Models;
    using MusicVideosRemote.Services;
    using MusicVideosRemote.ViewModels;
    using System;
    using System.Diagnostics;
    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ListAllVideosPage : ContentPage
    {
        public ListAllVideosPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // BindingContext = ListAllVideosViewModel.Current.Videos;
            CV.ItemsSource = ListAllVideosViewModel.Current.Videos;
        }

        private void CV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Video selected = (Video)e.CurrentSelection[0];
                SignalRClient.Current.QueueVideoAsync(selected.Id);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error CV_SelectionChanged: {ex.Message}");
            }
        }
    }
}