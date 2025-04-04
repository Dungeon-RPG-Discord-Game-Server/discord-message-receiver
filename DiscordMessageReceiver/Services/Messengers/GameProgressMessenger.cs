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

        public async Task UserRegisterAsync(ulong userId, int weaponType)
        {
            var initialPlayerData = new RegisterPlayerRequestDto
            {
                UserId = userId.ToString(),
                Name = userId.ToString(),
                WeaponType = weaponType
            };

            var response = await _apiWrapper.PostAsync(_gameServiceBaseUrl + "game/register", initialPlayerData);
            
            if (response == null)
            {
                Console.WriteLine($"❌ register POST 요청에 실패하였습니다: {userId}");
                return;
            }
            var status = JsonSerializerWrapper.Deserialize<RegisterPlayerResponseDto>(response);
            
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

        public async Task SendInitialWeaponChoiceButtonsAsync(ulong userId)
        {
            //TODO: 유저가 이미 등록되어 있는지 확인하는 로직 추가
            await SendMessageAsync(userId, "⚔️ Choose your weapon:", new ComponentBuilder()
                .WithButton("🗡️ Sword", "game_sword", ButtonStyle.Primary)
                .WithButton("🪄 MagicWand", "game_wand", ButtonStyle.Success));
        }

        /// <summary>
        /// 유저에게 버튼이 포함된 메인 메뉴 선택지 메시지를 DM으로 보냅니다.
        /// </summary>
        public async Task SendMainStateChoiceButtonsAsync(ulong userId)
        {
            await SendMessageAsync(userId, "🎮 What would you like to do?", new ComponentBuilder()
                .WithButton("▶ Continue Game", "game_continue_game", ButtonStyle.Primary)
                .WithButton("🆕 New Game", "game_new_game", ButtonStyle.Success)
                .WithButton("🛑 Quit Game", "game_quit_game", ButtonStyle.Danger));
        }

        /// <summary>
        /// 버튼 클릭 시 호출되는 이벤트 핸들러
        /// </summary>
        public override async Task OnButtonExecutedAsync(SocketMessageComponent interaction)
        {
            var user = interaction.User;
            string content = interaction.Data.CustomId switch
            {
                "game_continue_game" => "✅ You have selected **Continue Game**.\nPreparing to load your progress...",
                "game_new_game"      => "🆕 You have selected **New Game**.\nCreating a new adventure...",
                "game_quit_game"     => "🛑 You have selected **Quit Game**.\nHope to see you again soon!",
                "game_sword"         => "🗡️ You have selected **Sword**.\nPreparing to register your weapon...",
                "game_wand"          => "🪄 You have selected **Magic Wand**.\nPreparing to register your weapon...",
                _               => $"❌ You have selected an unknown option: **{interaction.Data.CustomId}**.\nPlease try again."
            };

            var builder = new ComponentBuilder(); // 버튼 제거

            await interaction.UpdateAsync(msg =>
            {
                msg.Content = content;
                msg.Components = builder.Build();
            });

            // 후속 비동기 작업은 여기서 실행
            switch (interaction.Data.CustomId)
            {
                case "game_sword":
                    await UserRegisterAsync(user.Id, 0);
                    break;
                case "game_wand":
                    await UserRegisterAsync(user.Id, 1);
                    break;
                case "game_continue_game":
                case "game_new_game":
                case "game_quit_game":
                    // TODO: 필요 시 처리 추가
                    break;
                default:
                    break;
            }

            // TODO: 선택 결과를 게임 서비스 API에 전달
        }
    }
}
