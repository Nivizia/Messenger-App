using ApiFetcher;
namespace ConsoleAppTest
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string username = "", email = "", password = "" ;
            string currentEmail = "", code = "";



            // lập trình đa luồng - xử lí bất đồng bộ trong C# :
            //https://viblo.asia/p/lap-trinh-bat-dong-bo-trong-c-DZrGNDoWkVB







            //------------------------[Hàm login]-----------------------------------
            var token = await AuthService.LoginAsync(username, password);
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


            Console.WriteLine(token);


        }
    }
}
