using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DiscordMessageReceiver.Services
{
    public class APIRequestWrapper
    {
        private readonly HttpClient _httpClient;
        private readonly ApiKeyManager _apiKeyManager;

        public APIRequestWrapper(HttpClient httpClient, ApiKeyManager apiKeyManager)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _apiKeyManager = apiKeyManager ?? throw new ArgumentNullException(nameof(apiKeyManager));
        }

        public async Task<string?> PostAsync(string url)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("X-API-KEY", await _apiKeyManager.GetValidApiKeyAsync());

                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    return responseText;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in PostAsync: {ex.Message}");
            }
        }

        public async Task<string?> PostAsync<T>(string url, T? payload)
        {
            try
            {
                HttpContent? content = null;

                if (payload != null)
                {
                    var json = JsonSerializer.Serialize(payload);
                    content = new StringContent(json, Encoding.UTF8, "application/json");
                }
                else
                {
                    content = new StringContent("", Encoding.UTF8, "application/json");
                }

                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = content
                };
                request.Headers.Add("X-API-KEY", await _apiKeyManager.GetValidApiKeyAsync());

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    return responseText;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in PostAsync: {ex.Message}");
            }
        }

        public async Task<string?> GetAsync(string url)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("X-API-KEY", await _apiKeyManager.GetValidApiKeyAsync());
                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    return responseText;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in GetAsync: {ex.Message}");
            }
        }

        public async Task<string?> PutAsync<T>(string url, T payload)
        {
            try
            {
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Put, url)
                {
                    Content = content
                };
                request.Headers.Add("X-API-KEY", await _apiKeyManager.GetValidApiKeyAsync());

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    return responseText;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in PutAsync: {ex.Message}");
            }
        }

        public async Task<string?> DeleteAsync(string url)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, url);
                request.Headers.Add("X-API-KEY", await _apiKeyManager.GetValidApiKeyAsync());

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    return responseText;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in DeleteAsync: {ex.Message}");
            }
        }
    }
}
