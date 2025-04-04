namespace GameStateService.Dtos
{
    public class RegisterPlayerResponseDto
    {
        public string PlayerId { get; set; }
        public bool Registered { get; set; }
        public string Message { get; set; }
    }
}
