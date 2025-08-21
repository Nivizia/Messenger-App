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

namespace MessengerUI
{
    /// <summary>
    /// Interaction logic for VerifyEmail.xaml
    /// </summary>
    public partial class VerifyEmail : Window
    {
        public string currentEmail = string.Empty;
        public string currentUsername = string.Empty;

        public VerifyEmail()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            string code = CodeTextBox.Text.Trim();

            var ConfirmVerifyBoolean = await AuthService.ConfirmEmailAsync(currentEmail, code);

            if (ConfirmVerifyBoolean)
            {
                MessageBox.Show("Email verification successful!");
                this.Hide();
                ContactWindow contactWindow = new ContactWindow();
                contactWindow.Username = currentUsername;
                contactWindow.Show();
            }
            else
            {
                MessageBox.Show("Email verification failed. Please check your code and try again.");
            }
        }
    }
}
