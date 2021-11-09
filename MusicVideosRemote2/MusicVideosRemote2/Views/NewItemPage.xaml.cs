namespace MusicVideosRemote2.Views
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    using MusicVideosRemote2.Models;
    using MusicVideosRemote2.ViewModels;

    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;

    public partial class NewItemPage : ContentPage
    {
        public Item Item
        {
            get; set;
        }

        public NewItemPage()
        {
            InitializeComponent();
            BindingContext = new NewItemViewModel();
        }
    }
}