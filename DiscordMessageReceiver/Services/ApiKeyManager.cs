using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace DiscordMessageReceiver.Services
{
    public class ApiKeyMetadata
    {
        public string Key { get; set; }
        public string Owner { get; set; }
        public DateTime Expiration { get; set; }
    }

    public class ApiKeyManager
    {
        private readonly IMemoryCache _cache;
        private readonly SecretClient _secretClient;
        private readonly HttpClient _httpClient;
        private readonly string _keyName;
        private readonly string _managerKey;
        private readonly string _gameServiceBaseUrl;

        public ApiKeyManager(IMemoryCache cache, IConfiguration config, string gameServiceBaseUrl, HttpClient httpClient)
        {
            _cache = cache;
            _httpClient = httpClient;
            _gameServiceBaseUrl = gameServiceBaseUrl;

            var keyVaultUri = new Uri(config["KeyVaultUri"]);
            _secretClient = new SecretClient(keyVaultUri, new DefaultAzureCredential());
            _keyName = "discord-bot";
            _managerKey = _secretClient.GetSecret("admin-api-key").Value.Value;
            Console.WriteLine($"🔑 Manager Key: {_managerKey}");
        }

        public async Task<string?> RequestApiKeyAsync(string url, string managerKey)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("X-MANAGER-KEY", managerKey);

                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    return responseText;
                }
                else
                {
                    throw new Exception($"POST request failed: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in PostAsync: {ex.Message}");
            }
        }

        public async Task<string> GetValidApiKeyAsync()
        {
            var cachedKey = _cache.TryGetValue("GameApiKey", out string cachedApiKey);
            if(!cachedKey)
            {
                await RequestApiKeyAsync(_gameServiceBaseUrl + $"apikey/generate?owner={_keyName}", _managerKey);
                var secret = await _secretClient.GetSecretAsync($"apikey-{_keyName}");
                var metadata = JsonSerializer.Deserialize<ApiKeyMetadata>(secret.Value.Value);

                var ttl = metadata.Expiration - DateTime.UtcNow - TimeSpan.FromMinutes(1); // 안전 여유
                _cache.Set("GameApiKey", metadata.Key, ttl);
                cachedApiKey = metadata.Key;
            }

            return cachedApiKey;
        }
    }
}
