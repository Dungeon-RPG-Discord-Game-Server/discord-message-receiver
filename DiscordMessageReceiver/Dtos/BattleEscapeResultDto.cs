namespace DiscordMessageReceiver.Dtos
{
    /// <summary>
    /// 전투 도망 결과를 나타내는 DTO 클래스입니다.
    /// </summary>
    public class BattleEscapeResultDto
    {
        /// <summary>
        /// 도망 성공 여부
        /// </summary>
        public bool IsEscaped { get; set; }

        public string Message { get; set; }
    }
}