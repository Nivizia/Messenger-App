using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApiFetcher
{
    public class UserDto
    {
        public string _id { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string profile_image { get; set; }
        public bool is_active { get; set; }
        public bool is_email_verification { get; set; }
        public string phone_number { get; set; }
        public string create_date { get; set; }
        public string wallet_id { get; set; }
        public int wrong_password_count { get; set; }
        public string login_lock_time { get; set; }
        public string role_id { get; set; }
    }

    public class MessageDto
    {
        public string _id { get; set; }
        public string conversation_id { get; set; }
        public string sender_id { get; set; }
        public string content { get; set; }
        public DateTime created_at { get; set; }
    }

    public class ApiResponseMessages
    {
        public bool success { get; set; }
        public List<List<MessageDto>> data { get; set; }   // chú ý: data là mảng 2 chiều [[]]
        public int length { get; set; }
        public string error { get; set; }
        public int error_code { get; set; }
    }


    public class ApiResponse
    {
        public bool success { get; set; }
        public List<UserDto> data { get; set; }
        public int error_code { get; set; }
        public string error { get; set; }
    }
    public class ApiResponsePost
    {
        public bool success { get; set; }
        public List<string> data { get; set; }
        public int length { get; set; }
        public string error { get; set; }
        public int error_code { get; set; }
    }
    public class ChatService
    {
        private static readonly HttpClient _httpClient = new();

        public static async Task<List<UserDto>> GetUsersAsync(string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
                "https://api.mmb.io.vn/py/api/chatbox/conservation/user-list");

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<ApiResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.data ?? new List<UserDto>();
        }

        public static async Task<List<UserDto>> SearchUsersAsync(string token, string searchData)
        {
            string url = $"https://api.mmb.io.vn/py/api/chatbox/conservation/search?request_data={searchData}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var apiResponse = JsonSerializer.Deserialize<ApiResponse>(json, options);
            return apiResponse?.data ?? new List<UserDto>();
        }

        public static async Task<List<string>?> CreateConversationAsync(string token, string userId)
        {
            string url = $"https://api.mmb.io.vn/py/api/chatbox/conversation/{userId}";

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = new StringContent(""); // tương đương `-d ''` trong curl

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var apiResponse = JsonSerializer.Deserialize<ApiResponsePost>(json, options);

            return apiResponse?.data != null && apiResponse.data.Count > 0 ? apiResponse.data : null;
        }
        public static async Task<List<MessageDto>?> GetMessagesAsync(string token, string conversationId, int skip = 0, int limit = 10)
        {
            string url = $"https://api.mmb.io.vn/py/api/chatbox/messages?id={conversationId}&skip={skip}&limit={limit}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var apiResponse = JsonSerializer.Deserialize<ApiResponseMessages>(json, options);

            // data trong response là List<List<MessageDto>> => mình lấy mảng đầu tiên
            return apiResponse?.data != null && apiResponse.data.Count > 0
                ? apiResponse.data[0]
                : null;
        }

        // TODO: Replace with real API when available
        // Placeholder method for sending messages - your friend can replace this implementation
        public static async Task<bool> SendMessageAsync(string token, string conversationId, string content)
        {
            // PLACEHOLDER IMPLEMENTATION
            // This simulates the API call that your friend will implement
            // For now, we just simulate a successful send after a short delay

            try
            {
                // Simulate network delay
                await Task.Delay(500);

                // TODO: Replace this entire method body with real API call:
                // string url = "https://api.mmb.io.vn/py/api/chatbox/send-message";
                // var payload = new { conversation_id = conversationId, content = content };
                // var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                // using var request = new HttpRequestMessage(HttpMethod.Post, url);
                // request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                // request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                // request.Content = jsonContent;
                // using var response = await _httpClient.SendAsync(request);
                // response.EnsureSuccessStatusCode();
                // var json = await response.Content.ReadAsStringAsync();
                // var apiResponse = JsonSerializer.Deserialize<ApiResponse>(json, options);
                // return apiResponse?.success ?? false;

                // For now, always return true to simulate successful sending
                return true;
            }
            catch (Exception)
            {
                // In case of any error, return false
                return false;
            }
        }

    }


}
