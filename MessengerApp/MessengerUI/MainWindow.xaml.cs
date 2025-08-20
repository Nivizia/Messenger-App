using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ApiFetcher;

namespace MessengerUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text.Trim();
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            string EmailPattern = @"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$";
            string UsernamePattern = @"^[^\s]{3,10}$";
            string PasswordPattern = @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$";

            bool isValidEmail = System.Text.RegularExpressions.Regex.IsMatch(email, EmailPattern);
            bool isValidUsername = System.Text.RegularExpressions.Regex.IsMatch(username, UsernamePattern);
            bool isValidPassword = System.Text.RegularExpressions.Regex.IsMatch(password, PasswordPattern);

            if (!isValidEmail)
            {
                MessageBox.Show("Invalid email format. Please enter a valid email.");
                return;
            }

            if (!isValidUsername)
            {
                MessageBox.Show("Invalid username format. Username must be 3-10 characters long and cannot contain spaces.");
                return;
            }

            if (!isValidPassword)
            {
                MessageBox.Show("Invalid password format. Password must be at least 8 characters long, contain at least one uppercase letter, one lowercase letter, one digit, and one special character.");
                return;
            }

            bool success = await AuthService.RegisterAsync(email, username, password);

            if (success)
            {
                MessageBox.Show("Registration successful");
            }
            else
            {
                MessageBox.Show("Registration fail");
            }
        }

        private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}