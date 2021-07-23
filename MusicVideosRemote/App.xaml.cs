﻿using MusicVideosRemote.Services;
using MusicVideosRemote.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MusicVideosRemote
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();
            // Startup the SignalR Client.
            SignalRClient.Current.RegisterAsync();

            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
