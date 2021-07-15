namespace MusicVideosRemote.Views
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MusicVideosRemote.Services;
    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;
    using MusicVideosRemote.Models;
    using MusicVideosRemote.ViewModels;
    using System.Diagnostics;
    using System.ComponentModel;


    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NowPlayingPage : ContentPage
    {
        public NowPlayingPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            BindingContext = NowplayingModel.Current;
        }
    }
}