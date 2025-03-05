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
            var envPath = "";
            Env.Load(envPath);
            // 환경 변수나 별도의 설정 파일에서 토큰을 불러오는 것을 권장합니다.
            Console.WriteLine("현재 작업 디렉터리: " + Environment.CurrentDirectory);
            string? token = Environment.GetEnvironmentVariable("BOT_TOKEN");
            Console.WriteLine($"Token: {token}");

            await _clientManger.InitClientAsync();
            await _clientManger.StartClientAsync(token);

            // 프로그램 종료 방지를 위한 무한 대기
            await Task.Delay(-1);
        }
    }
}