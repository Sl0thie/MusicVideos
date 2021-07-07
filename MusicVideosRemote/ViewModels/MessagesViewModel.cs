namespace MusicVideosRemote.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text;
    using System.Runtime.CompilerServices;
    using Xamarin.Forms;
    using MusicVideosRemote.Models;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using MusicVideosRemote.Services;
    using System.Diagnostics;

    public class MessagesViewModel : BaseViewModel
    {
        public ObservableCollection<MessageItem> Messages { get; }

        public Command LoadItemsCommand { get; }


        public MessagesViewModel()
        {
            Title = "Messages";
            Messages = new ObservableCollection<MessageItem>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
        }

        public void OnAppearing()
        {
            IsBusy = true;
        }

        async Task ExecuteLoadItemsCommand()
        {
            Debug.WriteLine("MessagesViewModel.ExecuteLoadItemsCommand");

            IsBusy = true;

            try
            {
                Messages.Clear();
                DataStore data = await DataStore.Instance;
                foreach (var item in data.Messages)
                {
                    Messages.Add(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }



    }
}