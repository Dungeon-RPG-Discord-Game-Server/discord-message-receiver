using Discord;
using Discord.Commands;
using Discord.WebSocket;

using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using DiscordMessageReceiver.Services;
using DiscordMessageReceiver.Dtos;
using DiscordMessageReceiver.Utils;

namespace DiscordMessageReceiver.Services.Messengers{
    public class GameProgressMessenger : BaseMessenger
    {
        private readonly IConfiguration _configuration;
        private readonly Logger _logger;
        public GameProgressMessenger(DiscordSocketClient client, APIRequestWrapper apiWrapper, string gameServiceBaseUrl, IConfiguration configuration) : base(client, apiWrapper, gameServiceBaseUrl)
        {
            _configuration = configuration;
            if (null == _configuration)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            string serviceName = configuration["Logging:ServiceName"];
            _logger = new Logger(serviceName);
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
                throw new UserErrorException($"{nameof(UserRegisterAsync)}: Failed to register user");
            }
            var status = JsonSerializerWrapper.Deserialize<RegisterPlayerResponseDto>(response);

            await SendMessageAsync(userId, status.Message);
        }

        public async Task LoadUserProgressAsync(ulong userId)
        {
            var response = await _apiWrapper.GetAsync(_gameServiceBaseUrl + "game/" + userId.ToString() + "/load");
            if (response == null)
            {
                throw new UserErrorException($"{nameof(LoadUserProgressAsync)}: Failed to load user progress");
            }

            var gameState = await GetPlayerGameStateAsync(userId);
            if (gameState == null)
            {
                throw new UserErrorException($"{nameof(LoadUserProgressAsync)}: Failed to get player game state");
            }

            switch (gameState)
            {
                case "MainMenuState":
                    await SendMainStateChoiceButtonsAsync(userId);
                    break;
                case "ExplorationState":
                    await ContiueExplorationAsync(userId);
                    break;
                case "BattleState":
                    await ContiueBattleAsync(userId);
                    break;
                default:
                    throw new UserErrorException($"{nameof(LoadUserProgressAsync)}: Unknown game state");
            }

            await SendMessageAsync(userId, response);
        }

        public async Task SendUserRegisterAsync(ulong userId)
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
            using(var log = _logger.StartMethod(nameof(OnButtonExecutedAsync))){
                try{
                    var user = interaction.User;

                    log.SetAttribute("button.type", nameof(GameProgressMessenger));
                    log.SetAttribute("button.userId", user.Id.ToString());
                    log.SetAttribute("button.customId", interaction.Data.CustomId);

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
                            await EnterDungeonAsync(user.Id);
                            break;
                        case "game_wand":
                            await UserRegisterAsync(user.Id, 1);
                            await EnterDungeonAsync(user.Id);
                            break;
                        case "game_continue_game":
                            //Player progress를 불러오는 API 호출
                        case "game_new_game":
                            await SendUserRegisterAsync(user.Id);
                            break;
                        case "game_quit_game":
                            // TODO: 필요 시 처리 추가
                            break;
                        default:
                            break;
                    }
                }catch (UserErrorException e)
                {
                    log.LogUserError(e.Message);
                }
                catch(Exception e)
                {
                    log.HandleException(e);
                }
            }
            // TODO: 선택 결과를 게임 서비스 API에 전달
        }
    }
}
