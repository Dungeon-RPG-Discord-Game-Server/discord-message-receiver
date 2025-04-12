using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscordMessageReceiver.Services
{
    public static class JsonSerializerWrapper
    {
        private static readonly JsonSerializerOptions _options = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false, // 필요 시 true 로 설정
            Converters = { new JsonStringEnumConverter() }
        };

        public static string Serialize<T>(T obj)
        {
            try
            {
                 return JsonSerializer.Serialize(obj, _options);
            }
            catch (Exception ex)
            {
                throw new Exception($"failed to serialize JSON: {ex.Message}");
            }
        }

        public static T? Deserialize<T>(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new Exception("JSON string is null or empty.");
            }

            try
            {
                return JsonSerializer.Deserialize<T>(json, _options);
            }
            catch (Exception ex)
            {
                throw new Exception($"failed to deserialize JSON: {ex.Message}");
            }
        }
    }
}
