using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using DiscordMessageReceiver.Dtos;

namespace DiscordMessageReceiver.Services.Messengers{
    public class AdventureMessenger : BaseMessenger
    {
        public AdventureMessenger(DiscordSocketClient client, APIRequestWrapper apiWrapper, string gameServiceBaseUrl) : base(client, apiWrapper, gameServiceBaseUrl)
        {
        }

        public async Task SendRoomChoiceButtonsAsync(ulong userId)
        {
            var response = await _apiWrapper.GetAsync(_gameServiceBaseUrl + $"game/{userId}/map/neighbors");
            var directions = JsonSerializerWrapper.Deserialize<string[]>(response);
            if (directions == null || directions.Length == 0)
            {
                Console.WriteLine($"❌ 유저의 방 정보를 가져올 수 없습니다: {userId}");
                return;
            }
            
            var component = new ComponentBuilder();

            foreach (var direction in directions)
            {
                string label = string.Empty;
                string id = "adventure_" + direction;
                switch (direction)
                {
                    case "up":
                        label = "⬆️ Up";
                        break;
                    case "down":
                        label = "⬇️ Down";
                        break;
                    case "left":
                        label = "⬅️ Left";
                        break;
                    case "right":
                        label = "➡️ Right";
                        break;
                }
                component.WithButton(label, id, ButtonStyle.Primary);
            }

            await SendMessageAsync(userId, "🏰 **Choose a room to enter:**\nSelect one of the available rooms below.", component);
        }

        public async Task MovePlayerAsync(MovePlayerRequestDto request)
        {

            var response = await _apiWrapper.PostAsync(_gameServiceBaseUrl + $"game/{request.UserId}/move", request);
            if (response == null)
            {
                Console.WriteLine($"❌ 유저의 이동 요청에 실패하였습니다: {request.UserId}");
                return;
            }

            Console.WriteLine($"✅ 유저의 이동 요청이 성공하였습니다: {request.UserId}");
        }

        /// <summary>
        /// 버튼 클릭 시 호출되는 이벤트 핸들러
        /// </summary>
        public override async Task OnButtonExecutedAsync(SocketMessageComponent interaction)
        {
            var user = interaction.User;
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
                    await SendMessageAsync(user.Id, await GetUserMapAsync(user.Id));
                    await SendRoomChoiceButtonsAsync(user.Id);
                    break;
                default:

                    break;
            }
        }
    }
}
