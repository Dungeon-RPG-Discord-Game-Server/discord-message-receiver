using Discord;
using Discord.Commands;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DiscordMessageReceiver.Services{
    public class ChoiceMessenger
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private const string GameServiceBaseUrl = "https://yourgameservice.example.com/api/";
        private readonly DiscordSocketClient _client;
        public ChoiceMessenger(DiscordSocketClient client)
        {
            _client = client;
            _client.ButtonExecuted += OnButtonExecutedAsync;
        }

        /// <summary>
        /// 유저에게 버튼이 포함된 선택지 메시지를 DM으로 보냅니다.
        /// </summary>
        public async Task SendBattleStateChoiceButtonsAsync(ulong userId)
        {
            var user = _client.GetUser(userId);
            if (user == null)
            {
                Console.WriteLine($"❌ 유저를 찾을 수 없습니다: {userId}");
                return;
            }

            var dm = await user.CreateDMChannelAsync();

            var component = new ComponentBuilder()
                .WithButton("⚔ 공격", customId: "choice_attack", ButtonStyle.Primary)
                .WithButton("🛡 방어", customId: "choice_defend", ButtonStyle.Secondary)
                .WithButton("🏃 도망", customId: "choice_run", ButtonStyle.Danger)
                .Build();

            await dm.SendMessageAsync("📜 당신의 선택은?", components: component);
            Console.WriteLine($"✅ 선택지를 {userId}에게 전송했습니다.");
        }

        public async Task SendMainStateChoiceButtonsAsync(ulong userId)
        {
            var user = _client.GetUser(userId);
            if (user == null)
            {
                Console.WriteLine($"❌ 유저를 찾을 수 없습니다: {userId}");
                return;
            }

            var dm = await user.CreateDMChannelAsync();

            var component = new ComponentBuilder()
                .WithButton("⚔ 공격", customId: "choice_attack", ButtonStyle.Primary)
                .WithButton("🛡 방어", customId: "choice_defend", ButtonStyle.Secondary)
                .WithButton("🏃 도망", customId: "choice_run", ButtonStyle.Danger)
                .Build();

            await dm.SendMessageAsync("📜 당신의 선택은?", components: component);
            Console.WriteLine($"✅ 선택지를 {userId}에게 전송했습니다.");
        }

        public async Task SendExplorationStateChoiceButtonsAsync(ulong userId)
        {
            var user = _client.GetUser(userId);
            if (user == null)
            {
                Console.WriteLine($"❌ 유저를 찾을 수 없습니다: {userId}");
                return;
            }

            var dm = await user.CreateDMChannelAsync();

            var component = new ComponentBuilder()
                .WithButton("⚔ 공격", customId: "choice_attack", ButtonStyle.Primary)
                .WithButton("🛡 방어", customId: "choice_defend", ButtonStyle.Secondary)
                .WithButton("🏃 도망", customId: "choice_run", ButtonStyle.Danger)
                .Build();

            await dm.SendMessageAsync("📜 당신의 선택은?", components: component);
            Console.WriteLine($"✅ 선택지를 {userId}에게 전송했습니다.");
        }

        /// <summary>
        /// 버튼 클릭 시 호출되는 이벤트 핸들러
        /// </summary>
        private async Task OnButtonExecutedAsync(SocketMessageComponent interaction)
        {
            var user = interaction.User;

            switch (interaction.Data.CustomId)
            {
                case "choice_attack":
                    await interaction.RespondAsync("⚔ 공격을 선택하셨습니다!", ephemeral: true);
                    Console.WriteLine($"{user.Username} 선택: 공격");
                    break;

                case "choice_defend":
                    await interaction.RespondAsync("🛡 방어를 선택하셨습니다!", ephemeral: true);
                    Console.WriteLine($"{user.Username} 선택: 방어");
                    break;

                case "choice_run":
                    await interaction.RespondAsync("🏃 도망을 선택하셨습니다!", ephemeral: true);
                    Console.WriteLine($"{user.Username} 선택: 도망");
                    break;

                default:
                    await interaction.RespondAsync("알 수 없는 선택입니다.", ephemeral: true);
                    break;
            }

            // TODO: 선택 결과를 게임 서비스 API에 전달하는 로직 추가
        }

        public class ChoiceOptionsPayload
        {
            public string userId { get; set; }
            public string[] options { get; set; }
        }
    }
}
