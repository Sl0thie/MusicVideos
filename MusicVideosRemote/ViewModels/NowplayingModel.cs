namespace MusicVideosRemote.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using MusicVideosRemote.Models;
    using MusicVideosRemote.Services;

    public class NowplayingModel : BaseViewModel
    {

        //readonly AsyncLazy<DataStore> data = DataStore.Instance;


        public NowplayingModel()
        {
            Title = "Now Playing";

        }



    }
}
