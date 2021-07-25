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
    /// FilteredVideosPage class.
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FilteredVideosPage : ContentPage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilteredVideosPage"/> class.
        /// </summary>
        public FilteredVideosPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// OnAppearing override for binding.
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();
            BindingContext = FilteredVideosViewModel.Current;
            CV.ItemsSource = FilteredVideosViewModel.Current.Videos;
        }

        /// <summary>
        /// Manages the selection changed event for the CV list.
        /// </summary>
        /// <param name="sender">Unused.</param>
        /// <param name="e">Unused value.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task CV_SelectionChangedAsync(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Video selected = (Video)e.CurrentSelection[0];
                await SignalRClient.Current.QueueVideoAsync(selected.Id);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error CV_SelectionChanged: {ex.Message}");
            }
        }
    }
}