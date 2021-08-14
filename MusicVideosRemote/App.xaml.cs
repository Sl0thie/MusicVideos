namespace MusicVideosRemote
{
    using System.Diagnostics;
    using MusicVideosRemote.Services;
    using Xamarin.Forms;

    /// <summary>
    /// App class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        public App()
        {
            InitializeComponent();

            // Startup the SignalR Client.
            _ = SignalRClient.Current.ConnectAsync();

            MainPage = new AppShell();
        }

        /// <summary>
        /// OnStart override. Currently unused.
        /// </summary>
        protected override void OnStart()
        {
        }

        /// <summary>
        /// OnSleep override. Currently unused.
        /// </summary>
        protected override void OnSleep()
        {
        }

        /// <summary>
        /// OnResume override. Currently unused.
        /// </summary>
        protected override void OnResume()
        {
        }
    }
}