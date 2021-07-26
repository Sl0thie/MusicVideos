namespace MusicVideosRemote.Views
{
    using System.Diagnostics;
    using MusicVideosRemote.ViewModels;
    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;

    /// <summary>
    /// FilterPage class.
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FilterPage : ContentPage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilterPage"/> class.
        /// </summary>
        public FilterPage()
        {
            Debug.WriteLine("FilterPage.FilterPage");

            InitializeComponent();
        }

        /// <summary>
        /// OnApearing override for binding.
        /// </summary>
        protected override void OnAppearing()
        {
            Debug.WriteLine("FilterPage.OnAppearing");

            base.OnAppearing();
            BindingContext = FilterViewModel.Current;
        }
    }
}