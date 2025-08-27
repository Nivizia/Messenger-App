using ApiFetcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Security.Cryptography;
using System.IO;

namespace MessengerUI
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            ConfirmButton.IsEnabled = false;

            string Username = UsernameTextBox.Text.Trim();
            string Password = PasswordBox.Password.Trim();

            string UsernamePattern = @"^[^\s]{3,20}$";
            string PasswordPattern = @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$";

            bool isValidUsername = System.Text.RegularExpressions.Regex.IsMatch(Username, UsernamePattern);
            bool isValidPassword = System.Text.RegularExpressions.Regex.IsMatch(Password, PasswordPattern);

            if (!isValidUsername)
            {
                MessageBox.Show("Invalid username format. Username must be 3-20 characters long and cannot contain spaces.");
                ConfirmButton.IsEnabled = true;
                return;
            }

            if (!isValidPassword)
            {
                MessageBox.Show("Invalid password format. Password must be at least 8 characters long, contain at least one uppercase letter, one lowercase letter, one digit, and one special character.");
                ConfirmButton.IsEnabled = true;
                return;
            }

            var token = await AuthService.LoginAsync(Username, Password);

            if (token != null)
            {
                MessageBox.Show("Login successful: ");

                ConfirmButton.IsEnabled = true;

                ContactWindow contactWindow = new ContactWindow(Username, token);
                contactWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Login failed. Please check your username and password.");
                ConfirmButton.IsEnabled = true;
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Hide();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Application is closing.");
            Application.Current.Shutdown();
        }
    }
}
