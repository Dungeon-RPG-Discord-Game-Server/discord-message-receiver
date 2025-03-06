using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace DiscordMessageReceiver.Commands
{
    // 게임 전용 커맨드 모듈: 게임 진행을 위한 커맨드 모듈 (모든 커맨드는 DM을 통해서만 입력받음)
    public class GameModule : ModuleBase<SocketCommandContext>
    {
        private bool IsMessageFromDM(){
            if (Context.Channel is IDMChannel){
                return true;
            }
            Console.WriteLine("이 커맨드는 반드시 DM을 통해서만 실행되어야 합니다.");
            return false;
        }

        [Command("save")]
        [Summary("게임의 현재 진행 상황을 저장합니다.")]
        public async Task SaveGameAsync(){
            // TODO
            if(IsMessageFromDM()){
                await ReplyAsync("게임을 성공적으로 저장하였습니다.");
            }
        }

        [Command("load")]
        [Summary("저장된 게임 진행 상황을 불러옵니다.")]
        public async Task LoadGameAsync(){
            // TODO
            if(IsMessageFromDM()){
                await ReplyAsync("게임을 성공적으로 불러왔습니다.");
            }
        }
    }
}