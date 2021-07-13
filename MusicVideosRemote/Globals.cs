namespace MusicVideosRemote
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Text;
    using MusicVideosRemote.Models;

    public static class Globals
    {
        private static Video currentVideo;

        [Bindable(true)]
        public static Video CurrentVideo
        {
            get { return currentVideo; }
            set 
            { 
                currentVideo = value;
                Debug.WriteLine($"Artist: {currentVideo.Artist}");
                Debug.WriteLine($"Title: {currentVideo.Title}");
            }
        }
    }
}