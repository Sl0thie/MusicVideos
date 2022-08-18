namespace MusicVideos
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    using MusicVideos.Hubs;

    /// <summary>
    /// This is the entry point of the ASP.net application. It contains application configuration related items.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">Represents a set of key/value application configuration properties.</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Configures services for the ASP.net application.
        /// </summary>
        /// <param name="services">Specifies the contract for a collection of service descriptors.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            _ = services.AddRazorPages();
            _ = services.AddSignalR(hubOptions =>
            {
                hubOptions.EnableDetailedErrors = true;
            });
        }

        /// <summary>
        /// Configures the ASP.net application.
        /// </summary>
        /// <param name="app">Defines a class that provides the mechanisms to configure an application's request pipeline.</param>
        /// <param name="env">Provides information about the web hosting environment an application is running in.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                _ = app.UseDeveloperExceptionPage();
            }
            else
            {
                _ = app.UseExceptionHandler("/Error");
            }

            _ = app.UseStaticFiles();
            _ = app.UseRouting();
            _ = app.UseAuthorization();

            //_ = app.UseEndpoints(endpoints =>
            //  {
            //      _ = endpoints.MapRazorPages();
            //      _ = endpoints.MapHub<VideoHub>("/videoHub");
            //      //_ = endpoints.MapHub<MessageHub>("/messageHub");
            //  });
        }
    }
}
