using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DiscordMessageReceiver.Services.Messengers{
    public class BattleMessenger : BaseMessenger
    {
        public BattleMessenger(DiscordSocketClient client, APIRequestWrapper apiWrapper, string gameServiceBaseUrl) : base(client, apiWrapper, gameServiceBaseUrl)
        {
        }

        /// <summary>
        /// 유저에게 버튼이 포함된 배틀 상태 선택지 메시지를 DM으로 보냅니다.
        /// </summary>
        public async Task SendBattleStateChoiceButtonsAsync(ulong userId)
        {
            await SendMessageAsync(userId, "⚔️ What would you like to do?", new ComponentBuilder()
                .WithButton("⚔ Attack", "choice_attack", ButtonStyle.Primary)
                .WithButton("🛡 Defend", "choice_defend", ButtonStyle.Success)
                .WithButton("🏃 Run", "choice_run", ButtonStyle.Danger));
        }

        /// <summary>
        /// 유저에게 버튼이 포함된 공격 타입입 선택지 메시지를 DM으로 보냅니다.
        /// </summary>
        public async Task SendAttackChoiceButtonsAsync(ulong userId)
        {
            await SendMessageAsync(userId, "⚔️ What type of attack would you like to use?", new ComponentBuilder()
                .WithButton("🗡 Normal Attack", "normal_attack", ButtonStyle.Primary)
                .WithButton("✨ Skill Attack", "skill_attack", ButtonStyle.Success));
        }

        /// <summary>
        /// 버튼 클릭 시 호출되는 이벤트 핸들러
        /// </summary>
        protected override async Task OnButtonExecutedAsync(SocketMessageComponent interaction)
        {
            var user = interaction.User;

            await interaction.UpdateAsync(msg =>
            {
                switch (interaction.Data.CustomId)
                {
                    // Battle State
                    case "choice_attack":
                        msg.Content = "⚔ You have selected **Attack**.\nPreparing your weapon...";
                        msg.Components = new ComponentBuilder().Build();
                        SendAttackChoiceButtonsAsync(user.Id).GetAwaiter().GetResult();
                        break;
                    case "choice_defend":
                        msg.Content = "🛡 You have selected **Defend**.\nBracing for impact...";
                        msg.Components = new ComponentBuilder().Build();
                        break;
                    case "choice_run":
                        msg.Content = "🏃 You have selected **Run**.\nAttempting to escape...";
                        msg.Components = new ComponentBuilder().Build();
                        break;
                    
                    // Attack Type
                    case "normal_attack":
                        msg.Content = "🗡 You have selected **Normal Attack**.\nReady to strike!";
                        msg.Components = new ComponentBuilder().Build();
                        break;
                    case "skill_attack":
                        msg.Content = "✨ You have selected **Skill Attack**.\nUnleashing your special ability!";
                        msg.Components = new ComponentBuilder().Build();
                        break;
                    default:
                        msg.Content = $"❌ You have selected an unknown option: **{interaction.Data.CustomId}**.\nPlease try again.";
                        msg.Components = new ComponentBuilder().Build();
                        break;
                }
            });

            // TODO: 선택 결과를 게임 서비스 API에 전달하는 로직 추가
        }
    }
}
