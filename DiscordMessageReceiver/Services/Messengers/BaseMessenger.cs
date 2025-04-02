using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DiscordMessageReceiver.Services.Messengers{
    public class BaseMessenger{
        protected readonly string _gameServiceBaseUrl;
        protected readonly APIRequestWrapper _apiWrapper;
        protected readonly DiscordSocketClient _client;
        public BaseMessenger(DiscordSocketClient client, APIRequestWrapper apiWrapper, string gameServiceBaseUrl)
        {
            _gameServiceBaseUrl = gameServiceBaseUrl;
            _apiWrapper = apiWrapper;
            _client = client;
            _client.ButtonExecuted += OnButtonExecutedAsync;
        }

        protected async Task<bool> CheckUserIsAliveAsync(ulong userId)
        {
            var response = await _apiWrapper.GetAsync(_gameServiceBaseUrl + "game/alive/" + userId);
            if (response == null)
            {
                Console.WriteLine($"❌ 유저를 찾을 수 없습니다: {userId}");
                return false;
            }

            var isAlive = JsonSerializer.Deserialize<bool>(response);
            if (isAlive)
            {
                return true;
            }
            else
            {
                Console.WriteLine($"❌ 현재 진행중인 유저 정보를 찾을 수 없습니다: {userId}");
                return false;
            }
        }

        protected async Task SendMessageAsync(ulong userId, string message, ComponentBuilder? component=null)
        {
            if (!await CheckUserIsAliveAsync(userId))
            {
                return;
            }
            var user = await _client.Rest.GetUserAsync(userId);
            if (user == null)
            {
                Console.WriteLine($"❌ 유저를 찾을 수 없습니다: {userId}");
                return;
            }

            var dm = await user.CreateDMChannelAsync();

            if (component == null)
            {
                await dm.SendMessageAsync(message);
                Console.WriteLine($"✅ 메세지지를 {userId}에게 전송했습니다.");
            }else
            {
                await dm.SendMessageAsync(message, components: component.Build());
                Console.WriteLine($"✅ 선택지를 {userId}에게 전송했습니다.");
            }
        }

        protected virtual Task OnButtonExecutedAsync(SocketMessageComponent interaction){
            // 기본 동작 또는 비워도 됨
            return Task.CompletedTask;
        }
    }
}