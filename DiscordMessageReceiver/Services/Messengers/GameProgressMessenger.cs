using Discord;
using Discord.WebSocket;

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
                await SendMessageAsync(userId, null);
                throw new UserErrorException($"{nameof(UserRegisterAsync)}: Failed to register user");
            }
            var status = JsonSerializerWrapper.Deserialize<RegisterPlayerResponseDto>(response);

            await SendMessageAsync(userId, status.Message);
        }

        public async Task LoadUserProgressAsync(ulong userId)
        {
            var response = await _apiWrapper.GetAsync(_gameServiceBaseUrl + "saveload/" + userId.ToString() + "/load");
            if (response == null)
            {
                await SendMessageAsync(userId, null);
                throw new UserErrorException($"{nameof(LoadUserProgressAsync)}: Failed to load user progress");
            }
            var userExist = JsonSerializerWrapper.Deserialize<bool>(response);
            string message = string.Empty;
            if (userExist)
            {
                message = "✅ Your progress has been successfully loaded.";
                await SendMessageAsync(userId, message);
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
            }else
            {
                message = "❌ Failed to load your progress. Please start a new game.";
                await SendMessageAsync(userId, message);
                await SendMainStateChoiceButtonsAsync(userId);
            }
        }

        public async Task SendUserRegisterAsync(ulong userId)
        {
            await SendMessageAsync(userId, "⚔️ Choose your weapon:", new ComponentBuilder()
                .WithButton("🗡️ Sword", "game_sword", ButtonStyle.Primary)
                .WithButton("🪄 MagicWand", "game_wand", ButtonStyle.Success));
        }

        public async Task SendMainStateChoiceButtonsAsync(ulong userId)
        {
            await SendMessageAsync(userId, "🎮 What would you like to do?", new ComponentBuilder()
                .WithButton("▶ Continue Game", "game_continue_game", ButtonStyle.Primary)
                .WithButton("🆕 New Game", "game_new_game", ButtonStyle.Success)
                .WithButton("🛑 Quit Game", "game_quit_game", ButtonStyle.Danger));
        }
        public override async Task OnButtonExecutedAsync(SocketMessageComponent interaction)
        {
            using (var log = _logger.StartMethod(nameof(OnButtonExecutedAsync)))
            {
                try
                {
                    var user = interaction.User;
                    var customId = interaction.Data.CustomId;

                    await interaction.DeferAsync();

                    log.SetAttribute("button.type", nameof(GameProgressMessenger));
                    log.SetAttribute("button.userId", user.Id.ToString());
                    log.SetAttribute("button.customId", customId);

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            string content = customId switch
                            {
                                "game_continue_game" => "✅ You have selected **Continue Game**.\nPreparing to load your progress...",
                                "game_new_game" => "🆕 You have selected **New Game**.\nCreating a new adventure...",
                                "game_quit_game" => "🛑 You have selected **Quit Game**.\nHope to see you again soon!",
                                "game_sword" => "🗡️ You have selected **Sword**.\nPreparing to register your weapon...",
                                "game_wand" => "🪄 You have selected **Magic Wand**.\nPreparing to register your weapon...",
                                _ => $"❌ You have selected an unknown option: **{customId}**.\nPlease try again."
                            };

                            await interaction.ModifyOriginalResponseAsync(msg =>
                            {
                                msg.Content = content;
                                msg.Components = new ComponentBuilder().Build();
                            });

                            switch (customId)
                            {
                                case "game_sword":
                                    await UserRegisterAsync(user.Id, 0);
                                    await EnterDungeonAsync(user.Id);
                                    break;
                                case "game_wand":
                                    await UserRegisterAsync(user.Id, 1);
                                    await EnterDungeonAsync(user.Id);
                                    break;
                                case "game_new_game":
                                    await SendUserRegisterAsync(user.Id);
                                    break;
                                case "game_continue_game":
                                    await LoadUserProgressAsync(user.Id);
                                    break;
                                case "game_quit_game":
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            log.HandleException(ex);
                        }
                    });
                }
                catch (Exception e)
                {
                    log.HandleException(e);
                }
            }
        }
    }
}
