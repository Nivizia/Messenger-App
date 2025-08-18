using ApiFetcher;
namespace ConsoleAppTest
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            //------------------------[Hàm login]-----------------------------------
            var token = await AuthService.LoginAsync(username, password);
            // gọi thằng này để truyền vào username và password -> nếu đúng nó sẽ trả token còn sai nó sẽ không trả gì
            //-----------------------------------------------------------------------

            //------------------------[hàm Register]---------------------------------
            var RegisterBoolean = await AuthService.RegisterAsync(email, username, password);
            // nhập email, username, password -> trả true hoặc false, khi áp dụng nhờ bắt validation cơ bản
            //-------------------------------------------------------------------------





            Console.WriteLine(token);


        }
    }
}
