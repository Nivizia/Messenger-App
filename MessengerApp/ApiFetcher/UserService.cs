using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApiFetcher
{
    public class UserService
    {
        public static async Task<string?> GetUserIdAsync(string token)
        {
            using (var client = new HttpClient())
            {
                // Gắn Authorization header
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Gọi API
                var response = await client.GetAsync("https://api.mmb.io.vn/py/api/chatbox/conservation/userid");

                if (!response.IsSuccessStatusCode)
                    return null; // lỗi HTTP thì trả null

                var json = await response.Content.ReadAsStringAsync();

                // Parse JSON
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("data", out var dataElem) && dataElem.GetArrayLength() > 0)
                {
                    return dataElem[0].GetString(); // lấy phần tử đầu tiên trong mảng data
                }
            }

            return null; // không có data
        }
    }
}
