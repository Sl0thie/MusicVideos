namespace MusicVideos.Hubs
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.SignalR;

    /// <summary>
    /// SignalR Hub to provide debugging over multiple pages.
    /// TODO Remove from production.
    /// </summary>
    public class DebugHub : Hub
    {
        /// <summary>
        /// Recieves error from page, posts to debug.
        /// </summary>
        /// <param name="name">Name of the error.</param>
        /// <param name="message">Message from the error.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task SendErrorAsync(string name, string message)
        {
            await Clients.All.SendAsync("PrintError", name, message);
        }

        /// <summary>
        /// Recieves message from page, posts to debug.
        /// </summary>
        /// <param name="message">Message from the error.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task SendMessageAsync(string message)
        {
            await Clients.All.SendAsync("PrintMessage",  message);
        }
    }
}
