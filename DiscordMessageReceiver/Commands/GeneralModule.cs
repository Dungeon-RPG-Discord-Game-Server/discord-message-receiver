using Discord.Commands;
using System.Threading.Tasks;

namespace DiscordMessageReceiver.Commands
{
    // 일반 명령어 모듈: 봇의 기본 커맨드를 정의합니다.
    public class GeneralModule : ModuleBase<SocketCommandContext>
    {
        // "!ping" 명령어: 봇 응답 속도 확인
        [Command("ping")]
        [Summary("Test service connection")]
        public async Task PingAsync()
        {
            Console.WriteLine("Pong!");
            await ReplyAsync("Pong!");
        }

        // "!echo" 명령어: 입력한 텍스트를 그대로 회신
        [Command("echo")]
        [Summary("입력한 텍스트를 그대로 출력합니다.")]
        public async Task EchoAsync([Remainder] string text)
        {
            await ReplyAsync(text);
        }
    }
}
