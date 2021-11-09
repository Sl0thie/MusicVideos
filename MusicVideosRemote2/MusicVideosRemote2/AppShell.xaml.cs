namespace MusicVideosRemote2
{
    using System;
    using System.Collections.Generic;

    using MusicVideosRemote2.ViewModels;
    using MusicVideosRemote2.Views;

    using Xamarin.Forms;

    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
            Routing.RegisterRoute(nameof(NewItemPage), typeof(NewItemPage));
        }

    }
}
