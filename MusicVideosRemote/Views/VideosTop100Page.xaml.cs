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
        /// <summary>
        /// Initializes a new instance of the <see cref="VideosTop100Page"/> class.
        /// </summary>
        public VideosTop100Page()
        {
            InitializeComponent();
        }

        /// <summary>
        /// OnAppearing override for binding.
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();
            BindingContext = new VideosTop100ViewModel();
        }

        ///// <summary>
        ///// Manages the selection changed event for the CV list.
        ///// </summary>
        ///// <param name="sender">Unused.</param>
        ///// <param name="e">Event arguments from the CV List.</param>
        //private void CV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    Video selected = (Video)e.CurrentSelection[0];
        //    _ = ProcessselectedAsync(selected.Id);
        //}

        ///// <summary>
        ///// Processes the item.
        ///// </summary>
        ///// <param name="id">The id of the video to process.</param>
        ///// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        //private async Task ProcessselectedAsync(int id)
        //{
        //    // TODO Replace with command.
        //    await SignalRClient.Current.QueueVideoAsync(id);
        //}
    }
}