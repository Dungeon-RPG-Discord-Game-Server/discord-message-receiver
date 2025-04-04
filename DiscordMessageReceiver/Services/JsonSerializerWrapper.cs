using System;
using System.Text.Json;

namespace DiscordMessageReceiver.Services
{
    public static class JsonSerializerWrapper
    {
        private static readonly JsonSerializerOptions _options = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false // 필요 시 true 로 설정
        };

        public static string Serialize<T>(T obj)
        {
            return JsonSerializer.Serialize(obj, _options);
        }

        public static T? Deserialize<T>(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                Console.WriteLine("⚠️ JSON 문자열이 null 또는 비어있습니다.");
                return default;
            }

            try
            {
                return JsonSerializer.Deserialize<T>(json, _options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ JSON 역직렬화 실패: {ex.Message}");
                return default;
            }
        }
    }
}
