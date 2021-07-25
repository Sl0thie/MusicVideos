namespace MusicVideosRemote.Views
{
    using System;
    using System.Diagnostics;
    using MusicVideosRemote.Models;
    using MusicVideosRemote.Services;
    using MusicVideosRemote.ViewModels;
    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FilteredVideosPage : ContentPage
    {
        public FilteredVideosPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            BindingContext = FilteredVideosViewModel.Current;
            CV.ItemsSource = FilteredVideosViewModel.Current.Videos;
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