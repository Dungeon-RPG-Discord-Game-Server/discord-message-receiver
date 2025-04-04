using Discord;
using Discord.Commands;
using Discord.WebSocket;

using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using DiscordMessageReceiver.Services;
using DiscordMessageReceiver.Dtos;

namespace DiscordMessageReceiver.Services.Messengers{
    public class GameProgressMessenger : BaseMessenger
    {
        public GameProgressMessenger(DiscordSocketClient client, APIRequestWrapper apiWrapper, string gameServiceBaseUrl) : base(client, apiWrapper, gameServiceBaseUrl)
        {
        }

        public async Task SendUserRegisterAsync(ulong userId)
        {
            var initialPlayerData = new RegisterPlayerRequestDto
            {
                UserId = userId.ToString(),
                Name = userId.ToString(),
            };

            var response = await _apiWrapper.PostAsync(_gameServiceBaseUrl + "game/register", initialPlayerData);

            Console.WriteLine($"{response}");
            
            if (response == null)
            {
                Console.WriteLine($"❌ register POST 요청에 실패하였습니다: {userId}");
                return;
            }
            var status = JsonSerializerWrapper.Deserialize<RegisterPlayerResponseDto>(response);

            Console.WriteLine($"{status.Registered}");
            
            if (status.Registered)
            {
                Console.WriteLine($"✅ 유저가 등록되었습니다: {userId}");
            }
            else
            {
                Console.WriteLine($"❌ 유저 등록에 실패했습니다: {userId}");
            }

            await SendMessageAsync(userId, status.Message);
        }

        /// <summary>
        /// 유저에게 버튼이 포함된 메인 메뉴 선택지 메시지를 DM으로 보냅니다.
        /// </summary>
        public async Task SendMainStateChoiceButtonsAsync(ulong userId)
        {
            await SendMessageAsync(userId, "🎮 What would you like to do?", new ComponentBuilder()
                .WithButton("▶ Continue Game", "continue_game", ButtonStyle.Primary)
                .WithButton("🆕 New Game", "new_game", ButtonStyle.Success)
                .WithButton("🛑 Quit Game", "quit_game", ButtonStyle.Danger));
        }

        /// <summary>
        /// 버튼 클릭 시 호출되는 이벤트 핸들러
        /// </summary>
        protected override async Task OnButtonExecutedAsync(SocketMessageComponent interaction)
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
                        _apiWrapper.PostAsync(_gameServiceBaseUrl+"choice/choice-response", payload = new
                        {
                            userId = user.Id.ToString(),
                            selectedOption = 1
                        }).GetAwaiter().GetResult();
                        break;
                    case "new_game":        
                        msg.Content = "🆕 You have selected **New Game**.\nCreating a new adventure...";
                        msg.Components = new ComponentBuilder().Build();
                        _apiWrapper.PostAsync(_gameServiceBaseUrl+"choice/choice-response", payload = new
                        {
                            userId = user.Id.ToString(),
                            selectedOption = 2
                        }).GetAwaiter().GetResult();
                        break;
                    case "quit_game":
                        msg.Content = "🛑 You have selected **Quit Game**.\nHope to see you again soon!";
                        msg.Components = new ComponentBuilder().Build();
                        _apiWrapper.PostAsync(_gameServiceBaseUrl+"choice/choice-response", payload = new
                        {
                            userId = user.Id.ToString(),
                            selectedOption = 3
                        }).GetAwaiter().GetResult();
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
