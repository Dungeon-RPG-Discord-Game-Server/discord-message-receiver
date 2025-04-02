using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DiscordMessageReceiver.Services{
    public class ChoiceMessenger
    {
        private const string GameServiceBaseUrl = "http://127.0.0.1:5048/api/";
        private readonly APIRequestWrapper _apiWrapper;
        private readonly DiscordSocketClient _client;
        public ChoiceMessenger(DiscordSocketClient client, APIRequestWrapper apiWrapper)
        {
            _apiWrapper = apiWrapper;
            _client = client;
            _client.ButtonExecuted += OnButtonExecutedAsync;
        }

        /// <summary>
        /// 유저에게 버튼이 포함된 메인 메뉴 선택지 메시지를 DM으로 보냅니다.
        /// </summary>
        public async Task SendMainStateChoiceButtonsAsync(ulong userId)
        {
            var user = await _client.Rest.GetUserAsync(userId);
            if (user == null)
            {
                Console.WriteLine($"❌ 유저를 찾을 수 없습니다: {userId}");
                return;
            }

            var dm = await user.CreateDMChannelAsync();

            var component = new ComponentBuilder()
                .WithButton("▶ Continue Game", "continue_game", ButtonStyle.Primary)      // ▶
                .WithButton("🆕 New Game", "new_game", ButtonStyle.Success)               // 🆕
                .WithButton("🛑 Quit Game", "quit_game", ButtonStyle.Danger)             // 🛑
                .Build();

            await dm.SendMessageAsync("🎮 What would you like to do?", components: component);
            Console.WriteLine($"✅ 선택지를 {userId}에게 전송했습니다.");
        }

        /// <summary>
        /// 유저에게 버튼이 포함된 배틀 상태 선택지 메시지를 DM으로 보냅니다.
        /// </summary>
        public async Task SendBattleStateChoiceButtonsAsync(ulong userId)
        {
            var user = await _client.Rest.GetUserAsync(userId);
            if (user == null)
            {
                Console.WriteLine($"❌ 유저를 찾을 수 없습니다: {userId}");
                return;
            }

            var dm = await user.CreateDMChannelAsync();

            var component = new ComponentBuilder()
                .WithButton("⚔ Attack", customId: "choice_attack", ButtonStyle.Primary)
                .WithButton("🛡 Defens", customId: "choice_defend", ButtonStyle.Success)
                .WithButton("🏃 Run", customId: "choice_run", ButtonStyle.Danger)
                .Build();

            await dm.SendMessageAsync("📜 당신의 선택은?", components: component);
            Console.WriteLine($"✅ 선택지를 {userId}에게 전송했습니다.");
        }

        /// <summary>
        /// 유저에게 버튼이 포함된 공격 타입입 선택지 메시지를 DM으로 보냅니다.
        /// </summary>
        public async Task SendAttackChoiceButtonsAsync(ulong userId)
        {
            var user = await _client.Rest.GetUserAsync(userId);
            if (user == null)
            {
                Console.WriteLine($"❌ 유저를 찾을 수 없습니다: {userId}");
                return;
            }

            var dm = await user.CreateDMChannelAsync();

            var component = new ComponentBuilder()
                .WithButton("🗡 Normal Attack", customId: "normal_attack", ButtonStyle.Primary)
                .WithButton("✨ Skill Attack", customId: "skill_attack", ButtonStyle.Success)
                .Build();

            await dm.SendMessageAsync("📜 당신의 선택은?", components: component);
            Console.WriteLine($"✅ 선택지를 {userId}에게 전송했습니다.");
        }

        public async Task SendExplorationStateChoiceButtonsAsync(ulong userId)
        {
            var user = await _client.Rest.GetUserAsync(userId);
            if (user == null)
            {
                Console.WriteLine($"❌ 유저를 찾을 수 없습니다: {userId}");
                return;
            }

            var dm = await user.CreateDMChannelAsync();

            var options = new[]
            {
                new { Label = "🚪 Room 1", Id = "room_1" },
                new { Label = "🚪 Room 2", Id = "room_2" },
                new { Label = "🚪 Room 3", Id = "room_3" }
            };
            
            var component = new ComponentBuilder();

            foreach (var opt in options)
            {
                component.WithButton(opt.Label, opt.Id, ButtonStyle.Primary);
            }

            component.Build();

            await dm.SendMessageAsync("🏰 **Choose a room to enter:**\nSelect one of the available rooms below.", components: component.Build());

            Console.WriteLine($"✅ 선택지를 {userId}에게 전송했습니다.");
        }

        /// <summary>
        /// 버튼 클릭 시 호출되는 이벤트 핸들러
        /// </summary>
        private async Task OnButtonExecutedAsync(SocketMessageComponent interaction)
        {
            var user = interaction.User;

            var payload = new
            {
                userId = user.Id.ToString(),
                selectedOption = 1
            };

            await interaction.UpdateAsync(msg =>
            {
                switch (interaction.Data.CustomId)
                {
                    // Main State
                    case "continue_game":
                        msg.Content = "✅ You have selected **Continue Game**.\nPreparing to load your progress...";
                        msg.Components = new ComponentBuilder().Build();
                        _apiWrapper.PostAsync(GameServiceBaseUrl+"choice/choice-response", payload = new
                        {
                            userId = user.Id.ToString(),
                            selectedOption = 1
                        }).GetAwaiter().GetResult();
                        break;
                    case "new_game":        
                        msg.Content = "🆕 You have selected **New Game**.\nCreating a new adventure...";
                        msg.Components = new ComponentBuilder().Build();
                        _apiWrapper.PostAsync(GameServiceBaseUrl+"choice/choice-response", payload = new
                        {
                            userId = user.Id.ToString(),
                            selectedOption = 2
                        }).GetAwaiter().GetResult();
                        break;
                    case "quit_game":
                        msg.Content = "🛑 You have selected **Quit Game**.\nHope to see you again soon!";
                        msg.Components = new ComponentBuilder().Build();
                        _apiWrapper.PostAsync(GameServiceBaseUrl+"choice/choice-response", payload = new
                        {
                            userId = user.Id.ToString(),
                            selectedOption = 3
                        }).GetAwaiter().GetResult();
                        break;
                    
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
                        // Exploration State
                        if (interaction.Data.CustomId.StartsWith("room"))
                        {
                            msg.Content = $"🚪 You have selected **{interaction.Data.Value}**.\nPreparing to enter the chamber...";
                            msg.Components = new ComponentBuilder().Build();
                        }
                        else
                        {
                            msg.Content = $"❌ You have selected an unknown option: **{interaction.Data.CustomId}**.\nPlease try again.";
                            msg.Components = new ComponentBuilder().Build();
                        }

                        break;
                }
            });

            // TODO: 선택 결과를 게임 서비스 API에 전달하는 로직 추가
        }

        public class ChoiceOptionsPayload
        {
            public string userId { get; set; }
            public string[] options { get; set; }
        }
    }
}
