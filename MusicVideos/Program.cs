using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace MusicVideos
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Model.LoadVideos();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
