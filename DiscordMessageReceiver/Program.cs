using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordMessageReceiver.Clients;
using System;
using System.Reflection;
using System.Threading.Tasks;
using DotNetEnv;

namespace DiscordMessageReceiver
{
    class Program
    {
        private IDiscordClientManager _clientManger;

        static async Task Main(string[] args) => await new Program().MainAsync();

        public async Task MainAsync()
        {
            _clientManger = new DiscordClientManager();
            
            Env.Load();
            string? token = Environment.GetEnvironmentVariable("BOT_TOKEN");

            await _clientManger.InitClientAsync();
            await _clientManger.StartClientAsync(token);

            // 프로그램 종료 방지를 위한 무한 대기
            await Task.Delay(-1);
        }
    }
}