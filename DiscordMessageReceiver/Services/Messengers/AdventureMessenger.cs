using Discord;
using Discord.WebSocket;

using DiscordMessageReceiver.Dtos;
using DiscordMessageReceiver.Utils;

namespace DiscordMessageReceiver.Services.Messengers{
    public class AdventureMessenger : BaseMessenger
    {
        private readonly IConfiguration _configuration;
        private readonly Logger _logger;
        public AdventureMessenger(DiscordSocketClient client, APIRequestWrapper apiWrapper, string gameServiceBaseUrl, IConfiguration configuration) : base(client, apiWrapper, gameServiceBaseUrl)
        {
            _configuration = configuration;
            if (null == _configuration)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            string serviceName = configuration["Logging:ServiceName"];
            _logger = new Logger(serviceName);
        }

        public async Task MovePlayerAsync(MovePlayerRequestDto request)
        {
            try
            {
                var response = await _apiWrapper.PostAsync(_gameServiceBaseUrl + $"game/{request.UserId}/move", request);
                if (response == null)
                {
                    await SendMessageAsync(ulong.Parse(request.UserId), null);
                    throw new UserErrorException($"{nameof(MovePlayerAsync)}: Failed to move player");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{nameof(MovePlayerAsync)}: {ex.Message}");
            }
        }

        public override async Task OnButtonExecutedAsync(SocketMessageComponent interaction)
        {
            await interaction.DeferAsync();

            _ = Task.Run(async () =>
            {
                using (var log = _logger.StartMethod(nameof(OnButtonExecutedAsync)))
                {
                    try
                    {
                        var user = interaction.User;
                        var customId = interaction.Data.CustomId;

                        log.SetAttribute("button.type", nameof(AdventureMessenger));
                        log.SetAttribute("button.userId", user.Id.ToString());
                        log.SetAttribute("button.customId", customId);

                        string content = customId switch
                        {
                            "adventure_up"    => "⬆️ You chose to move **up**. Heading north...",
                            "adventure_down"  => "⬇️ You chose to move **down**. Descending...",
                            "adventure_left"  => "⬅️ You chose to move **left**. Moving west...",
                            "adventure_right" => "➡️ You chose to move **right**. Moving east...",
                            _                 => "❓ Unknown direction. Please try again."
                        };

                        await interaction.ModifyOriginalResponseAsync(msg =>
                        {
                            msg.Content = content;
                            msg.Components = new ComponentBuilder().Build();
                        });

                        if (customId.StartsWith("adventure_"))
                        {
                            string direction = customId.Replace("adventure_", "");
                            var moveRequest = new MovePlayerRequestDto
                            {
                                UserId = user.Id.ToString(),
                                Direction = direction
                            };
                            await MovePlayerAsync(moveRequest);
                        }

                        var gameState = await GetPlayerGameStateAsync(user.Id);
                        switch (gameState)
                        {
                            case "MainMenuState":
                                break;
                            case "ExplorationState":
                                await ContiueExplorationAsync(user.Id);
                                break;
                            case "BattleState":
                                await StartBattleAsync(user.Id);
                                break;
                            default:
                                await SendMessageAsync(user.Id, "❌ Unknown game state.");
                                break;
                        }
                    }
                    catch (UserErrorException e)
                    {
                        log.LogUserError(e.Message);
                    }
                    catch (Exception e)
                    {
                        log.HandleException(e);
                    }
                }
            });
        }
    }
}
