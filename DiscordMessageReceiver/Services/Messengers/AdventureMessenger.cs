using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
                    throw new UserErrorException($"{nameof(MovePlayerAsync)}: Failed to move player");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{nameof(MovePlayerAsync)}: {ex.Message}");
            }
        }

        /// <summary>
        /// 버튼 클릭 시 호출되는 이벤트 핸들러
        /// </summary>
        public override async Task OnButtonExecutedAsync(SocketMessageComponent interaction)
        {
            using(var log = _logger.StartMethod(nameof(OnButtonExecutedAsync))){
                try{
                    var user = interaction.User;

                    log.SetAttribute("button.type", nameof(AdventureMessenger));
                    log.SetAttribute("button.userId", user.Id.ToString());
                    log.SetAttribute("button.customId", interaction.Data.CustomId);

                    var customId = interaction.Data.CustomId;

                    string content = customId switch
                    {
                        "adventure_up"    => "⬆️ You chose to move **up**. Heading north...",
                        "adventure_down"  => "⬇️ You chose to move **down**. Descending...",
                        "adventure_left"  => "⬅️ You chose to move **left**. Moving west...",
                        "adventure_right" => "➡️ You chose to move **right**. Moving east...",
                        _       => "❓ Unknown direction. Please try again."
                    };

                    var builder = new ComponentBuilder(); // 버튼 제거

                    await interaction.UpdateAsync(msg =>
                    {
                        msg.Content = content;
                        msg.Components = builder.Build();
                    });

                    // TODO: 선택 결과를 게임 서비스 API에 전달하는 로직 추가
                    switch (interaction.Data.CustomId)
                    {

                        case "adventure_up":
                        case "adventure_down":
                        case "adventure_left":
                        case "adventure_right":
                            string direction = interaction.Data.CustomId.Replace("adventure_", "");
                            var moveRequest = new MovePlayerRequestDto
                            {
                                UserId = user.Id.ToString(),
                                Direction = direction
                            };
                            await MovePlayerAsync(moveRequest);
                            break;
                        default:
                            break;
                    }

                    //만약 게임 스테이트가 배틀이면 배틀 실행
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
                }catch (UserErrorException e)
                {
                    log.LogUserError(e.Message);
                }
                catch(Exception e)
                {
                    log.HandleException(e);
                }
            }
        }
    }
}
