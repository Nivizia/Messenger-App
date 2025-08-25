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

    public class ApiResponse
    {
        public bool success { get; set; }
        public List<UserDto> data { get; set; }
        public int error_code { get; set; }
        public string error { get; set; }
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
    }
}
