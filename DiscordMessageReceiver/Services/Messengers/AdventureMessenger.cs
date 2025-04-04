using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DiscordMessageReceiver.Services.Messengers{
    public class AdventureMessenger : BaseMessenger
    {
        public AdventureMessenger(DiscordSocketClient client, APIRequestWrapper apiWrapper, string gameServiceBaseUrl) : base(client, apiWrapper, gameServiceBaseUrl)
        {
        }

        public async Task SendExplorationStateChoiceButtonsAsync(ulong userId)
        {
            var options = new[]
            {
                new { Label = "🚪 Room 1", Id = "adventure_room_1" },
                new { Label = "🚪 Room 2", Id = "adventure_room_2" },
                new { Label = "🚪 Room 3", Id = "adventure_room_3" }
            };
            
            var component = new ComponentBuilder();

            foreach (var opt in options)
            {
                component.WithButton(opt.Label, opt.Id, ButtonStyle.Primary);
            }

            await SendMessageAsync(userId, "🏰 **Choose a room to enter:**\nSelect one of the available rooms below.", component);
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
                _ => $"❌ You have selected an unknown option: **{customId}**.\nPlease try again."
            };

            var builder = new ComponentBuilder(); // 버튼 제거

            await interaction.UpdateAsync(msg =>
            {
                msg.Content = content;
                msg.Components = builder.Build();
            });

            // TODO: 선택 결과를 게임 서비스 API에 전달하는 로직 추가
        }
    }
}
