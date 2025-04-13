namespace DiscordMessageReceiver.Dtos
{
    public class MovePlayerRequestDto
    {
        public string UserId { get; set; } = default!;
        public string Direction { get; set; } = default!;
    }
}