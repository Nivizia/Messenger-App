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
    }

}
