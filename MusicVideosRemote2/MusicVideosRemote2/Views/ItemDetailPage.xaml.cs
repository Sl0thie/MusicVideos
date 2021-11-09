namespace MusicVideosRemote2.Views
{
    using System.ComponentModel;

    using MusicVideosRemote2.ViewModels;

    using Xamarin.Forms;

    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}