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
    }
}