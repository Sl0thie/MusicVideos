﻿namespace MusicVideosRemote.Views
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
            Debug.WriteLine("FilteredVideosPage.FilteredVideosPage");

            InitializeComponent();
        }

        /// <summary>
        /// OnAppearing override for binding.
        /// </summary>
        protected override void OnAppearing()
        {
            Debug.WriteLine("FilteredVideosPage.OnAppearing");

            base.OnAppearing();
            BindingContext = FilteredVideosViewModel.Current;
            CV.ItemsSource = FilteredVideosViewModel.Current.Videos;
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
                Debug.WriteLine("ListAllVideosPage.CV_SelectionChangedAsync");

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