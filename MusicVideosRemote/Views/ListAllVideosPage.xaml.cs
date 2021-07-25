namespace MusicVideosRemote.Views
{
    using MusicVideosRemote.ViewModels;
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

        }
    }
}