using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using DiscordMessageReceiver.Dtos;
using DiscordMessageReceiver.Services;

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
        }

        protected async Task<bool> CheckUserIsAOnlineAsync(ulong userId)
        {
            var response = await _apiWrapper.GetAsync(_gameServiceBaseUrl + $"game/{userId}/status");
            if (response == null)
            {
                Console.WriteLine($"❌ 유저를 찾을 수 없습니다: {userId}");
                return false;
            }

            var status = JsonSerializerWrapper.Deserialize<PlayerStatusDto>(response);
            if (status.Online)
            {
                Console.WriteLine($"✅ 유저가 현재 게임을 진행중입니다: {userId}");
                return true;
            }
            else
            {
                Console.WriteLine($"❌ 유저가 현재 게임을 진행중이지 않습니다: {userId}");
                return false;
            }
        }

        public async Task<string> GetUserSummaryAsync(ulong userId)
        {
            var response = await _apiWrapper.GetAsync(_gameServiceBaseUrl + $"game/{userId}/summary");
            if (response == null)
            {
                Console.WriteLine($"❌ 유저를 찾을 수 없습니다: {userId}");
                return string.Empty;
            }

            var summary = response;
            if (summary == null)
            {
                Console.WriteLine($"❌ 유저 요약 정보를 가져오는 데 실패했습니다: {userId}");
                return string.Empty;
            }

            return summary;
        }

        public async Task<string> GetUserMapAsync(ulong userId)
        {
            var response = await _apiWrapper.GetAsync(_gameServiceBaseUrl + $"game/{userId}/map");
            if (response == null)
            {
                Console.WriteLine($"❌ 유저를 찾을 수 없습니다: {userId}");
                return string.Empty;
            }

            var map = response;
            if (map == null)
            {
                Console.WriteLine($"❌ 유저 맵 정보를 가져오는 데 실패했습니다: {userId}");
                return string.Empty;
            }

            return map;
        }

        protected async Task SendMessageAsync(ulong userId, string message, ComponentBuilder? component=null)
        {
            // if (!await CheckUserIsAOnlineAsync(userId))
            // {
            //     return;
            // }
            var user = await _client.Rest.GetUserAsync(userId);
            if (user == null)
            {
                Console.WriteLine($"SendMessageAsync: ❌ 유저를 찾을 수 없습니다: {userId}");
                return;
            }

            var dm = await user.CreateDMChannelAsync();

            if (component == null)
            {
                await dm.SendMessageAsync(message);
                Console.WriteLine($"SendMessageAsync: ✅ 메세지를 {userId}에게 전송했습니다.");
            }else
            {
                await dm.SendMessageAsync(message, components: component.Build());
                Console.WriteLine($"SendMessageAsync: ✅ 선택지를 {userId}에게 전송했습니다.");
            }
        }

        public async Task<string> GetPlayerGameStateAsync(ulong userId)
        {
            var response = await _apiWrapper.GetAsync(_gameServiceBaseUrl + $"game/{userId}/state");
            if (response == null)
            {
                Console.WriteLine($"❌ 유저를 찾을 수 없습니다: {userId}");
                return string.Empty;
            }
            var gameState = response;
            if (gameState == null)
            {
                Console.WriteLine($"❌ 유저 게임 상태 정보를 가져오는 데 실패했습니다: {userId}");
                return string.Empty;
            }
            return gameState;
        }

        public async Task ContiueExplorationAsync(ulong userId)
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

        /// <summary>
        /// 유저에게 버튼이 포함된 배틀 상태 선택지 메시지를 DM으로 보냅니다.
        /// </summary>
        public async Task ContiueBattleAsync(ulong userId)
        {
            await SendMessageAsync(userId, "⚔️ What would you like to do?", new ComponentBuilder()
                .WithButton("⚔ Attack", "battle_choice_attack", ButtonStyle.Primary)
                .WithButton("🏃 Run", "battle_choice_run", ButtonStyle.Danger));
        }

        public virtual Task OnButtonExecutedAsync(SocketMessageComponent interaction){
            // 기본 동작 또는 비워도 됨
            Console.WriteLine($"[BaseMessenger] Button clicked: {interaction.Data.CustomId} by {interaction.User.Username}");
            return Task.CompletedTask;
        }
    }
}