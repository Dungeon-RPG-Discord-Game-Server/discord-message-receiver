using Discord.Commands;
using System.Threading.Tasks;

namespace DiscordMessageReceiver.Commands
{
    public class GeneralModule : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        [Summary("Test service connection")]
        public async Task PingAsync()
        {
            await ReplyAsync("Pong!");
        }

        [Command("echo")]
        [Summary("Echo the provided text")]
        public async Task EchoAsync([Remainder] string text)
        {
            await ReplyAsync(text);
        }
    }
}
