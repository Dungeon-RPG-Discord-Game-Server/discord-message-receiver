using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using DiscordMessageReceiver.Services;

namespace DiscordMessageReceiver.Commands
{
    // 게임 전용 커맨드 모듈: 게임 진행을 위한 커맨드 모듈 (모든 커맨드는 DM을 통해서만 입력받음)
    public class GameModule : ModuleBase<SocketCommandContext>
    {
        private readonly ChoiceMessenger _choiceMessenger;

        // 생성자를 통해 DI 주입
        public GameModule(ChoiceMessenger choiceMessenger)
        {
            _choiceMessenger = choiceMessenger;
        }

        [Command("choose")]
        [Summary("게임 서비스로부터 선택지를 받아 사용자에게 전송 후, 선택 결과를 게임 서비스에 전달합니다.")]
        public async Task ChooseAsync()
        {
            await _choiceMessenger.SendMainStateChoiceButtonsAsync(Context.User.Id);
        }

        [Command("room")]
        [Summary("게임 서비스로부터 선택지를 받아 사용자에게 전송 후, 선택 결과를 게임 서비스에 전달합니다.")]
        public async Task ChooseRoomAsync()
        {
            await _choiceMessenger.SendExplorationStateChoiceButtonsAsync(Context.User.Id);
        }

        [Command("battle")]
        [Summary("게임 서비스로부터 선택지를 받아 사용자에게 전송 후, 선택 결과를 게임 서비스에 전달합니다.")]
        public async Task ChooseBattleAsync()
        {
            await _choiceMessenger.SendBattleStateChoiceButtonsAsync(Context.User.Id);
        }
    }
}