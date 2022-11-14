// https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/windows-service?view=aspnetcore-6.0&tabs=visual-studio

using System.Net;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting.WindowsServices;

using MusicVideosService;
using MusicVideosService.Hubs;
using MusicVideosService.Services;

using Serilog;

// Setup logging for the application.
Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Debug()
    .WriteTo.File("MusicVideosService - .txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
Log.Information($"MusicVideosService Started: {DateTime.Now}");
Log.Information($"Environment CurrentDirectory: {Environment.CurrentDirectory}");

// Add config items.
Config.Application.TryAdd("DatabasePath", @"VideoData.db3");
Config.Application.TryAdd("ImportPath", @"F:\Music Videos for Import");
Config.Application.TryAdd("BasePath", @"F:\Music Videos");
Config.Application.TryAdd("ErrorPath", @"F:\Music Videos Errors");
Config.Application.TryAdd("VirtualPath", @"/Music Videos");
Config.Application.TryAdd("MinutesBetweenReplays", 10);

// Config web application.
WebApplicationOptions? options = new()
{
    Args = args,
    ContentRootPath = WindowsServiceHelpers.IsWindowsService() ? AppContext.BaseDirectory : default,
};

WebApplicationBuilder? builder = WebApplication.CreateBuilder(options);

// Add services.
builder.Services.AddRazorPages();

builder.Services.AddSignalR(o => o.EnableDetailedErrors = true);

builder.Services.AddSingleton<IDataStore, DataStore>(p =>
{
    DataStore dataStore = new DataStore();
    return dataStore;
});

//builder.Services.AddSingleton<IServer, Server>(p =>
//{
//    IDataStore dataStore = p.GetService<IDataStore>();
//    Server server = new Server(dataStore);
//    return server;
//});

builder.Services.AddHostedService<IServer>(p =>
{
    IDataStore? dataStore = p.GetService<IDataStore>();
    return new Server(dataStore);
});

builder.WebHost.ConfigureKestrel(configureOptions: (context, serverOptions) => serverOptions.Listen(IPAddress.Parse("192.168.0.6"), 933));

builder.Host.UseWindowsService();

WebApplication? app = builder.Build();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(@"F:\Music Videos"),
    RequestPath = "/Music Videos",
});

app.UseDirectoryBrowser(new DirectoryBrowserOptions
{
    FileProvider = new PhysicalFileProvider(@"F:\Music Videos"),
    RequestPath = "/Music Videos",
});

app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();

app.MapHub<DataHub>("/dataHub");

await app.RunAsync();