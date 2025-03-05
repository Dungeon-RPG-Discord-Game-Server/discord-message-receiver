using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;

namespace DiscordMessageReceiver.Clients{
    public class DiscordClientManager : IDiscordClientManager{
        public DiscordSocketClient Client { get; private set;}
        public CommandService Commands { get; private set; }

        public DiscordClientManager(){
            var clientCofig = new DiscordSocketConfig{
                LogLevel = LogSeverity.Info,
            };

            Client = new DiscordSocketClient(clientCofig);
            Client.Log += LogAsync;
            Client.MessageReceived += MessageReceivedAsync;

            Commands = new CommandService();
        }

        public async Task InitClientAsync(){
            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
        }

        public async Task StartClientAsync(string token){
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token cannot be null or empty.", nameof(token));

            await Client.LoginAsync(TokenType.Bot, token);
            await Client.StartAsync();
        }
        
        public async Task StopClientAsync(){
            //TODO
            await Client.StopAsync();
        }
        private async Task LogAsync(LogMessage logMessage)
        {
            Console.WriteLine(logMessage.ToString());
            await Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(SocketMessage messageParam)
        {
            // 봇 자신의 메시지나 다른 봇의 메시지는 무시합니다.
            if (!(messageParam is SocketUserMessage message)) return;
            if (message.Author.IsBot) return;

            int argPos = 0;
            // 예: '!' 접두사로 시작하는 경우에만 명령어로 인식
            if (!message.HasCharPrefix('!', ref argPos)) return;

            var context = new SocketCommandContext(Client, message);
            var result = await Commands.ExecuteAsync(context, argPos, null);
            if (!result.IsSuccess)
            {
                await context.Channel.SendMessageAsync(result.ErrorReason);
            }
        }
    }
}