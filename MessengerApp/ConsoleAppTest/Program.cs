using ApiFetcher;
using Microsoft.VisualBasic;
namespace ConsoleAppTest
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string username = "", email = "", password = "";
            string currentEmail = "", code = "";
            string searchData = "";
            //string userId = "";  



            // lập trình đa luồng - xử lí bất đồng bộ trong C# :
            //https://viblo.asia/p/lap-trinh-bat-dong-bo-trong-c-DZrGNDoWkVB
            //rong C#, async và await là hai từ khóa được sử dụng để đơn giản hóa việc viết code bất đồng bộ,
            //giúp tránh tình trạng block (treo) giao diện người dùng hoặc các luồng khác trong khi chờ một tác vụ dài hạn hoàn thành. 







            //------------------------[Hàm login]-----------------------------------
            var token = await AuthService.LoginAsync(username, password);
            Console.WriteLine("Access token: " + token);

            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Login failed or email chưa xác thực.");
                return;
            }

            // gọi thằng này để truyền vào username và password -> nếu đúng nó sẽ trả token còn sai nó sẽ không trả gì
            // lưu ý : hàm login nếu phát hiện tài khoản chưa xác thực email cũng sẽ từ chối cung cấp token
            //-----------------------------------------------------------------------

            //------------------------[hàm Register]---------------------------------
            var RegisterBoolean = await AuthService.RegisterAsync(email, username, password);
            



            // nhập email, username, password -> trả true hoặc false, khi áp dụng nhờ bắt validation cơ bản
            //-------------------------------------------------------------------------
            //------------------------[gửi yêu cầu xác thực email]---------------------
            var VerifyBoolean = await AuthService.VerifyEmailAsync(email);
            // nhập email vào -> nếu trả true thì hệ thống đã nhận mail -> nhận false thì nhập sai email hoặc email đã xác thực sẵn

            // lưu ý logic : 1 tài khoản chỉ được gửi mã xác nhận 1 lần sau 30 giây
            //               1 mã code tồn tại trong 20 phút
            //-------------------------------------------------------------------------

            //------------------------[nhập code xác thực email]-----------------------
            var ConfirmVerifyBoolean = await AuthService.ConfirmEmailAsync(currentEmail, code);
            // trả true hoặc false | điều kiện thì như ở trên
            //-------------------------------------------------------------------------
            //-------------------------[ hàm lấy tất cả user trả về object]-----------
            //var token2 = "your_token_here";
            var _servive = new ChatService();
            Console.WriteLine("\nAll Users:");

            var users = await ChatService.GetUsersAsync(token);

            foreach (var user in users)
            {
                Console.WriteLine($"{user.username} - {user.email} - Role: {user.role_id}");
            }
            //--------------------------------------------------------------------------
            //------------------------------[các service liên quan tới conversation và tin nhắn]
            //search người dùng (theo username hoặc email)
         var  searchUsers = await ChatService.SearchUsersAsync(token, searchData);
            

            if (searchUsers.Count == 0)
            {
                Console.WriteLine("No user found.");
                return;
            }

            foreach (var user in searchUsers)
            {
                Console.WriteLine($"{user.username} - {user.email} - ID: {user._id}");
            }

            //----------------- [Tạo conversation + Lấy message]----------
            var userId = searchUsers[0]._id; // lấy user đầu tiên
            var conversationIds = await ChatService.CreateConversationAsync(token, userId);

            if (conversationIds == null || !conversationIds.Any())
            {
                Console.WriteLine("Không tạo được phòng chat.");
                return;
            }


            //data có trả id user dùng nó để tạo phòng chat
            //string? conversationId = await ChatService.CreateConversationAsync(token, userId);

            //List<string> conversationIds = await ChatService.CreateConversationAsync(token, userId);
            //string conversationId = conversationIds.FirstOrDefault(); // Lấy 1 ID đầu tiên nếu có

            string conversationId = conversationIds.First();
            Console.WriteLine($"\nConversation ID created: {conversationId}");

            //sau khi có phòng chat rồi thì load các tin nhắn cũ
            List<MessageDto>? messages = await ChatService.GetMessagesAsync(token, conversationId);

            // chức năng chat trực tiếp hiện vẫn còn đang dev, dự kiến nối nay xong

        }
    }
}
