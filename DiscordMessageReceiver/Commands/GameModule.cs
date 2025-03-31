using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DiscordMessageReceiver.Commands
{
    // 게임 전용 커맨드 모듈: 게임 진행을 위한 커맨드 모듈 (모든 커맨드는 DM을 통해서만 입력받음)
    public class GameModule : ModuleBase<SocketCommandContext>
    {
        private static readonly HttpClient _httpClient = new HttpClient();
    private const string GameServiceBaseUrl = "https://yourgameservice.example.com/api/";

    [Command("choose")]
    [Summary("게임 서비스로부터 선택지를 받아 사용자에게 전송 후, 선택 결과를 게임 서비스에 전달합니다.")]
    public async Task ChooseAsync()
    {
        // 1. 게임 서비스 API로부터 선택지 가져오기
        string userId = Context.User.Id.ToString();
        string getUrl = $"{GameServiceBaseUrl}choice/choice-options?userId={userId}";
        HttpResponseMessage response = await _httpClient.GetAsync(getUrl);
        if (!response.IsSuccessStatusCode)
        {
            await ReplyAsync("선택지를 가져오는데 실패했습니다.");
            return;
        }

        string json = await response.Content.ReadAsStringAsync();
        var choiceOptions = JsonSerializer.Deserialize<ChoiceOptionsPayload>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (choiceOptions == null || choiceOptions.options == null)
        {
            await ReplyAsync("유효한 선택지가 없습니다.");
            return;
        }

        // 2. 사용자에게 선택지 전송 (간단히 텍스트로 출력)
        string optionsText = string.Join("\n", choiceOptions.options);
        await ReplyAsync($"선택지를 받았습니다:\n{optionsText}\n숫자로 선택지를 입력해주세요.");

        // 3. 사용자의 응답 대기 (간단한 메시지 핸들러로 처리)
        var userResponse = await NextMessageAsync(timeout: System.TimeSpan.FromSeconds(30));
        if (userResponse == null)
        {
            await ReplyAsync("시간이 초과되었습니다.");
            return;
        }

        if (int.TryParse(userResponse.Content, out int selectedOption))
        {
            // 4. 선택 결과를 게임 서비스 API에 전송
            var choiceResponse = new { userId, selectedOption };
            string jsonPayload = JsonSerializer.Serialize(choiceResponse);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            HttpResponseMessage postResponse = await _httpClient.PostAsync($"{GameServiceBaseUrl}choice/choice-response", content);
            if (postResponse.IsSuccessStatusCode)
            {
                await ReplyAsync("선택이 성공적으로 처리되었습니다.");
            }
            else
            {
                await ReplyAsync("선택 결과를 전송하는 데 실패했습니다.");
            }
        }
        else
        {
            await ReplyAsync("유효한 숫자를 입력해주세요.");
        }
    }

    // 간단한 메시지 대기 함수 (이전 예제 참고)
    private async Task<SocketMessage> NextMessageAsync(System.TimeSpan timeout)
    {
        var tcs = new TaskCompletionSource<SocketMessage>();

        Task Handler(SocketMessage message)
        {
            if (message.Channel.Id == Context.Channel.Id && message.Author.Id == Context.User.Id)
            {
                tcs.TrySetResult(message);
            }
            return Task.CompletedTask;
        }

        Context.Client.MessageReceived += Handler;

        var delayTask = Task.Delay(timeout);
        var completedTask = await Task.WhenAny(tcs.Task, delayTask);
        Context.Client.MessageReceived -= Handler;

        if (completedTask == tcs.Task)
        {
            return await tcs.Task;
        }
        else
        {
            return null;
        }
    }

    public class ChoiceOptionsPayload
    {
        public string userId { get; set; }
        public string[] options { get; set; }
    }
    }
}