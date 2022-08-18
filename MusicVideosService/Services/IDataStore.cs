namespace MusicVideosService.Services
{
    using MusicVideosService.Models;

    public interface IDataStore
    {
        Video SelectVideoFromId(int id);

        Task InsertOrUpdateVideo(Video video);

        Task UpdateVideoPropertiesAsync(int id, int duration, int width, int height);
    }
}
