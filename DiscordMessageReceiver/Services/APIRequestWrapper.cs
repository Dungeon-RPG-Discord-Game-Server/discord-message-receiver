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

        public APIRequestWrapper(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string?> PostAsync(string url)
        {
            try
            {
                var response = await _httpClient.PostAsync(url, null);
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

                var response = await _httpClient.PostAsync(url, content);

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

        public async Task<string?> GetAsync(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    return responseText;
                }
                else
                {
                    throw new Exception($"GET request failed: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in PostAsync: {ex.Message}");
            }
        }

        public async Task<string?> PutAsync<T>(string url, T payload)
        {
            try
            {
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    return responseText;
                }
                else
                {
                    throw new Exception($"PUT request failed: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in PostAsync: {ex.Message}");
            }
        }

        public async Task<string?> DeleteAsync(string url)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    return responseText;
                }
                else
                {
                    throw new Exception($"DELETE request failed: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in PostAsync: {ex.Message}");
            }
        }
    }
}
