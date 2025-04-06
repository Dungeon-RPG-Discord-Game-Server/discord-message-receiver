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
        /// 유저에게 버튼이 포함된 공격 타입입 선택지 메시지를 DM으로 보냅니다.
        /// </summary>
        public async Task SendAttackChoiceButtonsAsync(ulong userId)
        {
            await SendMessageAsync(userId, "⚔️ What type of attack would you like to use?", new ComponentBuilder()
                .WithButton("🗡 Normal Attack", "battle_normal_attack", ButtonStyle.Primary)
                .WithButton("✨ Skill Attack", "battle_skill_attack", ButtonStyle.Success));
        }

        /// <summary>
        /// 버튼 클릭 시 호출되는 이벤트 핸들러
        /// </summary>
        public override async Task OnButtonExecutedAsync(SocketMessageComponent interaction)
        {
            var user = interaction.User;
            var customId = interaction.Data.CustomId;

            // 1. 메시지 수정 내용 준비
            string content = customId switch
            {
                "choice_attack"   => "⚔ You have selected **Attack**.\nPreparing your weapon...",
                "choice_defend"   => "🛡 You have selected **Defend**.\nBracing for impact...",
                "choice_run"      => "🏃 You have selected **Run**.\nAttempting to escape...",
                "normal_attack"   => "🗡 You have selected **Normal Attack**.\nReady to strike!",
                "skill_attack"    => "✨ You have selected **Skill Attack**.\nUnleashing your special ability!",
                _                 => $"❌ You have selected an unknown option: **{customId}**.\nPlease try again."
            };

            // 2. 버튼 제거하고 메시지 수정
            await interaction.UpdateAsync(msg =>
            {
                msg.Content = content;
                msg.Components = new ComponentBuilder().Build();
            });

            // 3. 후속 비동기 로직 (버튼 추가 등)
            if (customId == "battle_choice_attack")
            {
                await SendAttackChoiceButtonsAsync(user.Id);  // ⚠️ 이건 반드시 UpdateAsync 외부에서 호출해야 함
            }

            // TODO: 추가로 게임 상태 업데이트 등 처리 가능
        }
    }
}
