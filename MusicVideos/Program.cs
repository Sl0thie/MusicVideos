namespace MusicVideos
{
    using System;
    using LogCore3;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// Program class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="args">Arguments to pass to the host builder.</param>
        public static void Main(string[] args)
        {
            Log.Start(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Visual Studio 2019\\Logs", true, true, false);
            DS.Initialize();

            // Model.LoadSettings();
            // Model.LoadVideos();
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Creates the host for the application.
        /// </summary>
        /// <param name="args">Arguments passed from the main entry point.</param>
        /// <returns>A program initialization abstraction.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
.ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
        }
    }
}
