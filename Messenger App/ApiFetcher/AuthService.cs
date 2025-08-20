using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;

namespace ApiFetcher
{

    public class AuthService
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<string?> LoginAsync(string username, string password)
        {
            var url = "https://api.mmb.io.vn/py/api/user/auth/login";

            var formData = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("username", username),
            new KeyValuePair<string, string>("password", password),
            new KeyValuePair<string, string>("scope", ""),
            new KeyValuePair<string, string>("client_id", "string"),
            new KeyValuePair<string, string>("client_secret", "string")
            });

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("accept", "application/json");

            var response = await client.PostAsync(url, formData);
            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("is_email_verification", out var isVerified) is false)
                {
                    return null;
                }
                if (doc.RootElement.TryGetProperty("access_token", out var token))
                {
                    return token.GetString();
                }
            }

            return null; // đăng nhập thất bại
        }


        public static async Task<bool> RegisterAsync(string email, string username, string password)
        {
            var url = "https://api.mmb.io.vn/py/api/user/auth/register";

            var payload = new
            {
                email = email,
                username = username,
                password = password
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("accept", "application/json");

            var response = await client.PostAsync(url, jsonContent);
            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("success", out var successProp))
                {
                    return successProp.GetBoolean();
                }
            }

            return false; // đăng ký thất bại
        }

        public static async Task<bool> VerifyEmailAsync(string email)
        {
            try
            {
                // Tạo URL với query param email
                string url = $"https://api.mmb.io.vn/py/api/user/email/verify?email={Uri.EscapeDataString(email)}";

                // Tạo request POST
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("accept", "application/json");

                // Vì API không yêu cầu body nên gửi rỗng
                request.Content = new StringContent("");

                // Gửi request
                HttpResponseMessage response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                // Đọc response body
                string responseBody = await response.Content.ReadAsStringAsync();

                // Parse JSON
                using var doc = JsonDocument.Parse(responseBody);
                if (doc.RootElement.TryGetProperty("success", out JsonElement successElement))
                {
                    return successElement.GetBoolean();
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        public static async Task<bool> ConfirmEmailAsync(string currentEmail, string code)
        {
            try
            {
                string url = $"https://api.mmb.io.vn/py/api/user/email/confirm?code={Uri.EscapeDataString(code)}&current_email={Uri.EscapeDataString(currentEmail)}";

                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("accept", "application/json");
                request.Content = new StringContent("");

                HttpResponseMessage response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(responseBody);
                if (doc.RootElement.TryGetProperty("success", out JsonElement successElement))
                {
                    return successElement.GetBoolean();
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }

}
