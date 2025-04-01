using Discord.WebSocket;
using Discord.Commands;

namespace DiscordMessageReceiver.Clients{
    public interface IDiscordClientManager{
        Task InitClientAsync();

        Task StartClientAsync(string token);

        Task StopClientAsync();
    }
}