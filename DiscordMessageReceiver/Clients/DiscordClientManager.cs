using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordMessageReceiver.Clients
{
    public class DiscordClientManager : IDiscordClientManager
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        public DiscordClientManager(
            DiscordSocketClient client,
            CommandService commands,
            IServiceProvider services)
        {
            _client = client;
            _commands = commands;
            _services = services;

            _client.Log += LogAsync;
            _client.MessageReceived += MessageReceivedAsync;
        }

        public async Task InitClientAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        public async Task StartClientAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token cannot be null or empty.", nameof(token));

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
        }

        public async Task StopClientAsync()
        {
            await _client.StopAsync();
        }

        private async Task LogAsync(LogMessage logMessage)
        {
            Console.WriteLine(logMessage.ToString());
            await Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(SocketMessage messageParam)
        {
            if (!(messageParam is SocketUserMessage message)) return;
            if (message.Author.IsBot) return;

            Console.WriteLine($"Received message: {message.Content}");

            int argPos = 0;
            if (!message.HasCharPrefix('!', ref argPos))
            {
                Console.WriteLine("접두사가 감지되지 않았습니다.");
                return;
            }

            var context = new SocketCommandContext(_client, message);
            var result = await _commands.ExecuteAsync(context, argPos, _services);
            if (!result.IsSuccess)
            {
                await context.Channel.SendMessageAsync(result.ErrorReason);
            }
        }
    }
}
