namespace MusicVideosRemote2.Views
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using MusicVideosRemote2.Models;
    using MusicVideosRemote2.ViewModels;
    using MusicVideosRemote2.Views;

    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;

    public partial class ItemsPage : ContentPage
    {
        ItemsViewModel _viewModel;

        public ItemsPage()
        {
            InitializeComponent();

            BindingContext = _viewModel = new ItemsViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }
    }
}