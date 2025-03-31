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
        var user = _client.GetUser(ulong.Parse(payload.UserId));
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
