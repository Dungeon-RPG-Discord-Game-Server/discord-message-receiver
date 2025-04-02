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
                new { Label = "🚪 Room 1", Id = "room_1" },
                new { Label = "🚪 Room 2", Id = "room_2" },
                new { Label = "🚪 Room 3", Id = "room_3" }
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
        protected override async Task OnButtonExecutedAsync(SocketMessageComponent interaction)
        {
            var user = interaction.User;

            await interaction.UpdateAsync(msg =>
            {
                switch (interaction.Data.CustomId)
                {
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
