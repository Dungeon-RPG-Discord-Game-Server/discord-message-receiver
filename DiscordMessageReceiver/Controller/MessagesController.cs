using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly DiscordSocketClient _client;

    public MessagesController(DiscordSocketClient client)
    {
        _client = client;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessageToUser([FromBody] MessagePayload payload)
    {
        // Truoble: 유저 아이디가 존재하지만 찾아지지 않는경우
        // GetUser 함수가 아닌 GetUserAsync를 사용하여 비동기적으로 유저 정보를 가져옵니다.
        // GetUser 함수는 현재 그룹 내부에 있는 유저만 가져올 수 있습니다.
        // GetUserAsync는 느리지만 유저 아이디만을 이용해 유저를 가져올 수 있습니다.
        var user = await _client.Rest.GetUserAsync(ulong.Parse(payload.UserId));
        if (user == null) return NotFound("User not found");

        var dm = await user.CreateDMChannelAsync();
        await dm.SendMessageAsync(payload.Message);

        return Ok("메시지 전송 완료");
    }

    public class MessagePayload
    {
        public string UserId { get; set; }
        public string Message { get; set; }
    }
}
