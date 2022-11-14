namespace MusicVideosService
{
    using System.Threading;
    using System.Timers;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Management.Infrastructure;
    using MusicVideosService.Hubs;
    using Serilog;

    public class Worker : BackgroundService
    {
        private static System.Timers.Timer timer;
        private static IHubContext<DataHub> hubContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="Worker"/> class.
        /// </summary>
        /// <param name="hub">A reference to the SignalR hub.</param>
        public Worker(IHubContext<DataHub> hub)
        {
            Log.Information("Worker Constructor");

            try
            {
                hubContext = hub;
                timer = new System.Timers.Timer(60000);
                timer.Elapsed += Timer_Elapsed;
                timer.AutoReset = true;
                timer.Enabled = true;
                timer.Start();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            // Log.Information("Worker Timer_Elapsed");
            try
            {
                _ = hubContext.Clients.All.SendAsync("Message", $"test message {DateTime.Now}");
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }

        /// <summary>
        /// ExecuteAsync method is called when the Microsoft.Extensions.Hosting.IHostedService starts. The implementation should return a task that represents the lifetime of the long running operation(s) being performed.
        /// </summary>
        /// <param name="stoppingToken">Triggered when Microsoft.Extensions.Hosting.IHostedService.StopAsync(System.Threading.CancellationToken) is called.</param>
        /// <returns>A System.Threading.Tasks.Task that represents the long running operations.</returns>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}