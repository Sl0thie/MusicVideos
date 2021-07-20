namespace MusicVideosRemote.Views
{
    using MusicVideosRemote.ViewModels;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

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
            BindingContext = ListAllVideosViewModel.Current.Videos;
        }

        private void CV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}