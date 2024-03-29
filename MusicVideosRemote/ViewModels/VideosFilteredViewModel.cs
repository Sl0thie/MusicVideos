﻿namespace MusicVideosRemote.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using MusicVideosRemote.Models;
    using MusicVideosRemote.Services;
    using MusicVideosRemote.Views;

    /// <summary>
    /// VideosFilteredViewModel class.
    /// </summary>
    internal class VideosFilteredViewModel : BaseViewModel
    {
        /// <summary>
        /// Gets or sets the Videos collection.
        /// </summary>
        public ObservableCollection<Video> Videos
        {
            get
            {
                return videos;
            }

            set
            {
                videos = value;
                OnPropertyChanged("Videos");
            }
        }

        /// <summary>
        /// Gets or sets the Total Videos.
        /// </summary>
        public string TotalVideos
        {
            get
            {
                return totalVideos;
            }

            set
            {
                totalVideos = value;
                OnPropertyChanged("TotalVideos");
            }
        }

        private ObservableCollection<Video> videos = new ObservableCollection<Video>();
        private string totalVideos;

        /// <summary>
        /// Initializes a new instance of the <see cref="VideosFilteredViewModel"/> class.
        /// </summary>
        public VideosFilteredViewModel()
        {
            _ = LoadVideosAsync();
        }

        /// <summary>
        /// Updates a video in the collection.
        /// </summary>
        /// <param name="video">The video to update.</param>
        public void UpdateVideo(Video video)
        {
            if (Settings.PassFilter(video))
            {
                if (videos.Any(vid => vid.Id == video.Id))
                {
                    Video existing = videos.First(vid => vid.Id == video.Id);
                    int index = videos.IndexOf(existing);
                    Videos[index] = video;
                }
                else
                {
                    Videos.Add(video);
                }
            }
            else
            {
                if (videos.Any(vid => vid.Id == video.Id))
                {
                    Video existing = videos.First(vid => vid.Id == video.Id);
                    int index = videos.IndexOf(existing);
                    Videos.RemoveAt(index);
                }
            }

            TotalVideos = $"{videos.Count} videos";
        }

        /// <summary>
        /// Method to update the collection when the filter is updated.
        /// </summary>
        public void UpdateFilter()
        {
            _ = LoadVideosAsync();
        }

        /// <summary>
        /// Loads the Videos list with video objects.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task LoadVideosAsync()
        {
            DataStore database = await DataStore.Instance;
            Videos = new ObservableCollection<Video>(await database.GetFilteredVideosAsync(Settings.Filter));
            TotalVideos = $"{videos.Count} videos";
        }
    }
}