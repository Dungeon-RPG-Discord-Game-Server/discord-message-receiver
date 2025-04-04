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

        [Command("register")]
        [Summary("게임 서비스에 유저를 등록합니다.")]
        public async Task RegisterAsync()
        {
            await _gameProgressMessenger.SendUserRegisterAsync(Context.User.Id);
        }

        [Command("choose")]
        [Summary("게임 서비스로부터 선택지를 받아 사용자에게 전송 후, 선택 결과를 게임 서비스에 전달합니다.")]
        public async Task ChooseAsync()
        {
            await _gameProgressMessenger.SendMainStateChoiceButtonsAsync(Context.User.Id);
        }

        [Command("room")]
        [Summary("게임 서비스로부터 선택지를 받아 사용자에게 전송 후, 선택 결과를 게임 서비스에 전달합니다.")]
        public async Task ChooseRoomAsync()
        {
            await _adventureMessenger.SendExplorationStateChoiceButtonsAsync(Context.User.Id);
        }

        [Command("battle")]
        [Summary("게임 서비스로부터 선택지를 받아 사용자에게 전송 후, 선택 결과를 게임 서비스에 전달합니다.")]
        public async Task ChooseBattleAsync()
        {
            await _battleMessenger.SendBattleStateChoiceButtonsAsync(Context.User.Id);
        }
    }
}