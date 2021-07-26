namespace MusicVideosRemote
{
    using System.Diagnostics;

    /// <summary>
    /// AppShell class.
    /// </summary>
    public partial class AppShell : Xamarin.Forms.Shell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppShell"/> class.
        /// </summary>
        public AppShell()
        {
            Debug.WriteLine("AppShell.AppShell");

            InitializeComponent();
        }
    }
}