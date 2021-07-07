namespace MusicVideosRemote.Views
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;
    using MusicVideosRemote.ViewModels;
    using System.Diagnostics;

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MessagesPage : ContentPage
    {

        MessagesViewModel _viewModel;

        public MessagesPage()
        {
            Debug.WriteLine("MessagesPage.MessagesPage");


            InitializeComponent();

            BindingContext = _viewModel = new MessagesViewModel();


        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }

    }
}