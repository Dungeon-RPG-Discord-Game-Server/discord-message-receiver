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

        public async Task<string?> PostAsync<T>(string url, T payload)
        {
            try
            {
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"✅ DELETE 요청 성공: {url}\n응답: {responseText}");
                    return responseText;
                }
                else
                {
                    Console.WriteLine($"❌ POST 실패: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❗ 예외 발생: {ex.Message}");
                return null;
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
                    Console.WriteLine($"✅ DELETE 요청 성공: {url}\n응답: {responseText}");
                    return responseText;
                }
                else
                {
                    Console.WriteLine($"❌ GET 실패: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❗ 예외 발생: {ex.Message}");
                return null;
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
                    Console.WriteLine($"✅ DELETE 요청 성공: {url}\n응답: {responseText}");
                    return responseText;
                }
                else
                {
                    Console.WriteLine($"❌ PUT 실패: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❗ 예외 발생: {ex.Message}");
                return null;
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
                    Console.WriteLine($"✅ DELETE 요청 성공: {url}\n응답: {responseText}");
                    return responseText;
                }
                else
                {
                    Console.WriteLine($"❌ DELETE 실패: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❗ 예외 발생: {ex.Message}");
                return null;
            }
        }
    }
}
