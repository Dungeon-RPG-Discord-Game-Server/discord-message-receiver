using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using DiscordMessageReceiver.Services;
using DiscordMessageReceiver.Services.Messengers;

namespace DiscordMessageReceiver.Commands
{
    // 게임 전용 커맨드 모듈: 게임 진행을 위한 커맨드 모듈 (모든 커맨드는 DM을 통해서만 입력받음)
    public class GameModule : ModuleBase<SocketCommandContext>
    {
        private readonly GameProgressMessenger _gameProgressMessenger;
        private readonly AdventureMessenger _adventureMessenger;
        private readonly BattleMessenger _battleMessenger;

        // 생성자를 통해 DI 주입
        public GameModule(GameProgressMessenger gameProgressMessenger, AdventureMessenger adventureMessenger, BattleMessenger battleMessenger)
        {
            _gameProgressMessenger = gameProgressMessenger;
            _adventureMessenger = adventureMessenger;
            _battleMessenger = battleMessenger;
        }

        [Command("game")]
        [Summary("게임을 시작하기 위해 사용자에게 DM을 전송합니다.")]
        public async Task OpenDMAsync()
        {
            var dmChannel = await Context.User.CreateDMChannelAsync();
            
            await dmChannel.SendMessageAsync("게임을 시작합니다.");
            await ReplyAsync("게임을 위한 DM을 전송 하였습니다. !register 명령어를 이용해 게임을 시작하세요.");
        }

        [Command("start")]
        [Summary("게임을 시작합니다.")]
        public async Task StartGameAsync()
        {
            
        }

        [Command("summary")]
        [Summary("게임을 시작합니다.")]
        public async Task UserSummaryAsync()
        {
            string? response = await _gameProgressMessenger.GetUserSummaryAsync(Context.User.Id);
            if (response!=null)
            {
                await ReplyAsync(response);
                return;
            }else
            {
                await ReplyAsync("게임 서비스와 연결할 수 없습니다.");
                return;
            }
        }

        [Command("register")]
        [Summary("게임 서비스에 유저를 등록합니다.")]
        public async Task RegisterAsync()
        {
            await _gameProgressMessenger.SendUserRegisterAsync(Context.User.Id);
        }
    }
}