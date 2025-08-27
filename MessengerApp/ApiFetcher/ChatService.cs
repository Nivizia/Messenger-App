using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ApiFetcher
{
    public class UserDto
    {
        public string _id { get; set; } = "";
        public string username { get; set; } = "";
        public string email { get; set; } = "";
        public string profile_image { get; set; } = "";
        public bool is_active { get; set; } 
        public bool is_email_verification { get; set; } 
        public string phone_number { get; set; } = "";
        public string create_date { get; set; } = "";
        public string wallet_id { get; set; } = "";
        public int wrong_password_count { get; set; }
        public string login_lock_time { get; set; } = "";
        public string role_id { get; set; } = "";
        public string? ConversationId { get; set; }     
        [JsonIgnore]
        public string UserId => _id;
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
            const int MAX = 50; // giới hạn trả về
            var keyword = (searchData ?? string.Empty).Trim();
            if (keyword.Length == 0) return new List<UserDto>();

            // --- 1) Gọi search API như cũ ---
            //string url = $"https://api.mmb.io.vn/py/api/chatbox/conservation/search?request_data={searchData}";

            string url = $"https://api.mmb.io.vn/py/api/chatbox/conservation/search?request_data={Uri.EscapeDataString(keyword)}";


            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await _httpClient.SendAsync(request);
            var jsonResponse = await response.Content.ReadAsStringAsync();
            //response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();


            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };


            List<UserDto>? source = null;
            if (response.IsSuccessStatusCode)
            {
                var api = JsonSerializer.Deserialize<ApiResponse>(jsonResponse, options);
                if (api?.data != null && api.data.Count > 0)
                {
                    
                    source = api.data;
                }
                // nếu API trả rỗng -> Fallback
            }
            if (source == null)
                source = await GetUsersAsync(token);


            bool StartsWith(UserDto u) =>
    (!string.IsNullOrEmpty(u.username) && u.username.StartsWith(keyword, StringComparison.OrdinalIgnoreCase)) ||
    (!string.IsNullOrEmpty(u.email) && u.email.StartsWith(keyword, StringComparison.OrdinalIgnoreCase));

            bool Contains(UserDto u) =>
                (!string.IsNullOrEmpty(u.username) && u.username.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(u.email) && u.email.Contains(keyword, StringComparison.OrdinalIgnoreCase));

            var result =
                source
                .Where(u => StartsWith(u) || Contains(u))
                .GroupBy(u => u._id)              // loại trùng
                .Select(g => g.First())
                .OrderByDescending(StartsWith)     // ưu tiên khớp đầu
                .ThenBy(u => u.username ?? string.Empty)
                .Take(MAX)
                .ToList();

            return result;
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

        /*public static async Task<bool> SendMessageAsync(string token, string conversationId, string content)
        {
            if (string.IsNullOrWhiteSpace(token)) throw new ArgumentException("token is required");
            if (string.IsNullOrWhiteSpace(conversationId)) throw new ArgumentException("conversationId is required");
            if (string.IsNullOrWhiteSpace(content)) return false;

            // ⚠️ CHỌN 1 TRONG 2 endpoint tuỳ backend của bạn:

            // (A) Thường gặp: POST /message  { conversation_id, content }
            var url = "https://api.mmb.io.vn/py/api/chatbox/message";
            var payload = new { conversation_id = conversationId, content = content };

            // // (B) Nếu backend của bạn dùng /messages:
            // var url = "https://api.mmb.io.vn/py/api/chatbox/messages";
            // var payload = new { id = conversationId, content = content };

            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            using var res = await _httpClient.SendAsync(req);
            return res.IsSuccessStatusCode;
        }*/

        public static async Task<bool> SendMessageAsync(string token, string conversationId, string content)
        {
            if (string.IsNullOrWhiteSpace(token)) throw new ArgumentException("token is required");
            if (string.IsNullOrWhiteSpace(conversationId)) throw new ArgumentException("conversationId is required");
            if (string.IsNullOrWhiteSpace(content)) return false;

            var attempts = new List<(HttpMethod method, string url, HttpContent body)>
    {
        // ❖ NHÓM A – /messages với query ?id= (ưu tiên PUT vì POST đang bị 405)
        (HttpMethod.Put,  $"https://api.mmb.io.vn/py/api/chatbox/messages?id={Uri.EscapeDataString(conversationId)}",
            JsonContent(new { content })),
        (HttpMethod.Put,  $"https://api.mmb.io.vn/py/api/chatbox/messages?id={Uri.EscapeDataString(conversationId)}",
            FormContent(new Dictionary<string,string>{{"content", content}})),

        (HttpMethod.Post, $"https://api.mmb.io.vn/py/api/chatbox/messages?id={Uri.EscapeDataString(conversationId)}",
            JsonContent(new { content })),
        (HttpMethod.Post, $"https://api.mmb.io.vn/py/api/chatbox/messages?id={Uri.EscapeDataString(conversationId)}",
            FormContent(new Dictionary<string,string>{{"content", content}})),

        // ❖ NHÓM B – /messages không query, truyền id trong body
        (HttpMethod.Put,  "https://api.mmb.io.vn/py/api/chatbox/messages",
            JsonContent(new { id = conversationId, content })),
        (HttpMethod.Put,  "https://api.mmb.io.vn/py/api/chatbox/messages",
            JsonContent(new { conversation_id = conversationId, content })),
        (HttpMethod.Post, "https://api.mmb.io.vn/py/api/chatbox/messages",
            JsonContent(new { id = conversationId, content })),
        (HttpMethod.Post, "https://api.mmb.io.vn/py/api/chatbox/messages",
            JsonContent(new { conversation_id = conversationId, content })),

        // ❖ NHÓM C – path có {id}
        (HttpMethod.Put,  $"https://api.mmb.io.vn/py/api/chatbox/messages/{Uri.EscapeDataString(conversationId)}",
            JsonContent(new { content })),
        (HttpMethod.Post, $"https://api.mmb.io.vn/py/api/chatbox/messages/{Uri.EscapeDataString(conversationId)}",
            JsonContent(new { content })),
    };

            var logs = new StringBuilder();
            foreach (var (method, url, body) in attempts)
            {
                var (ok, status, resp) = await Send(token, method, url, body);
                if (ok) return true;
                logs.AppendLine($"{method} {url} => {status} {Trim(resp)}");
            }

            throw new Exception("Send failed:\n" + logs.ToString());
        }

        // ==== helpers ====
        private static StringContent JsonContent(object obj) =>
            new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

        private static FormUrlEncodedContent FormContent(IDictionary<string, string> form) =>
            new FormUrlEncodedContent(form);

        private static async Task<(bool ok, int status, string body)> Send(
            string token, HttpMethod method, string url, HttpContent content)
        {
            using var req = new HttpRequestMessage(method, url);
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            req.Content = content;

            using var res = await _httpClient.SendAsync(req);
            var body = await res.Content.ReadAsStringAsync();
            return (res.IsSuccessStatusCode, (int)res.StatusCode, body);
        }

        private static string Trim(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            s = s.Replace("\r", "").Replace("\n", " ");
            return s.Length > 200 ? s.Substring(0, 200) + " ..." : s;
        }

        public static async Task<string> DiagnoseWsHttpAsync(string token, string conversationId)
        {
            var urls = new[]
            {
        "https://api.mmb.io.vn/py/api/chatbox/ws",
        $"https://api.mmb.io.vn/py/api/chatbox/ws?token={Uri.EscapeDataString(token)}",
        $"https://api.mmb.io.vn/py/api/chatbox/ws?token={Uri.EscapeDataString(token)}&conversation_id={Uri.EscapeDataString(conversationId)}",
    };

            var sb = new StringBuilder();
            foreach (var u in urls)
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, u);
                req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                req.Headers.Add("Origin", "https://api.mmb.io.vn");
                var res = await _httpClient.SendAsync(req);
                var body = await res.Content.ReadAsStringAsync();
                sb.AppendLine($"{u} -> {(int)res.StatusCode} {body}");
            }
            return sb.ToString();
        }






    }


}
