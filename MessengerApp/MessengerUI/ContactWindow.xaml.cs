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
using ApiFetcher;
using System.IO;
using System.Security.Cryptography;

namespace MessengerUI
{
    /// <summary>
    /// Interaction logic for ContactWindow.xaml
    /// </summary>
    public partial class ContactWindow : Window
    {
        public string Username = string.Empty;
        private string currentToken = string.Empty;
        private List<UserDto> allUsers = new List<UserDto>();
        private List<UserDto> filteredUsers = new List<UserDto>();
        private UserDto? currentUser = null; // Store current user info for chat

        public ContactWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UsernamePlaceholder.Text = $"Logged in as: {Username}";

            // Load saved token
            LoadToken();

            // Load current user info using service token approach
            await LoadCurrentUser();

            // Load all users automatically
            await LoadAllUsers();
        }

        private async Task LoadCurrentUser()
        {
            try
            {
                if (string.IsNullOrEmpty(currentToken))
                {
                    StatusLabel.Text = "No token available for user lookup";
                    StatusLabel.Foreground = new SolidColorBrush(Colors.Orange);
                    return;
                }

                // Use the service token approach to get current user info
                currentUser = await ChatService.GetCurrentUserAsync(currentToken);

                if (currentUser != null)
                {
                    StatusLabel.Text = $"Current user found: {currentUser.username} (ID: {currentUser._id})";
                    StatusLabel.Foreground = new SolidColorBrush(Colors.Green);
                    Console.WriteLine($"[DEBUG] Successfully loaded current user: {currentUser.username} (ID: {currentUser._id})");
                    return;
                }

                // If we still don't have current user info
                StatusLabel.Text = "Error: Could not find current user. Check your login credentials.";
                StatusLabel.Foreground = new SolidColorBrush(Colors.Red);
                Console.WriteLine($"[DEBUG] Failed to load current user for username: {Username}");
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"Error loading current user: {ex.Message}";
                StatusLabel.Foreground = new SolidColorBrush(Colors.Red);
                Console.WriteLine($"[DEBUG] LoadCurrentUser error: {ex.Message}");
            }
        }

        private void LoadToken()
        {
            try
            {
                if (File.Exists("auth.token"))
                {
                    byte[] encryptedToken = File.ReadAllBytes("auth.token");
                    byte[] tokenBytes = ProtectedData.Unprotect(encryptedToken, null, DataProtectionScope.CurrentUser);
                    currentToken = Encoding.UTF8.GetString(tokenBytes);
                    StatusLabel.Text = "Token loaded successfully";
                }
                else
                {
                    StatusLabel.Text = "No token found. Please login again.";
                    StatusLabel.Foreground = new SolidColorBrush(Colors.Red);
                    MessageBox.Show("Authentication token not found. Please login again.", "Authentication Required",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"Error loading token: {ex.Message}";
                StatusLabel.Foreground = new SolidColorBrush(Colors.Red);
                MessageBox.Show($"Error loading authentication token: {ex.Message}", "Authentication Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadAllUsers()
        {
            try
            {
                StatusLabel.Text = "Loading users...";
                StatusLabel.Foreground = new SolidColorBrush(Colors.Blue);

                allUsers = await ChatService.GetUsersAsync(currentToken);
                filteredUsers = new List<UserDto>(allUsers);

                UserListBox.ItemsSource = filteredUsers;

                StatusLabel.Text = $"Loaded {allUsers.Count} users";
                StatusLabel.Foreground = new SolidColorBrush(Colors.Green);
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"Error loading users: {ex.Message}";
                StatusLabel.Foreground = new SolidColorBrush(Colors.Red);
                MessageBox.Show($"Failed to load users: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            await PerformSearch();
        }

        private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Perform search as user types (with small delay to avoid too many calls)
            await Task.Delay(300);
            await PerformSearch();
        }

        private async Task PerformSearch()
        {
            try
            {
                string searchText = SearchTextBox.Text.Trim();

                if (string.IsNullOrEmpty(searchText))
                {
                    // Show all users if search is empty
                    filteredUsers = new List<UserDto>(allUsers);
                    UserListBox.ItemsSource = filteredUsers;
                    StatusLabel.Text = $"Showing all {allUsers.Count} users";
                    StatusLabel.Foreground = new SolidColorBrush(Colors.Green);
                    return;
                }

                StatusLabel.Text = "Searching...";
                StatusLabel.Foreground = new SolidColorBrush(Colors.Blue);

                // Use API search
                var searchResults = await ChatService.SearchUsersAsync(currentToken, searchText);
                filteredUsers = searchResults;
                UserListBox.ItemsSource = filteredUsers;

                StatusLabel.Text = $"Found {searchResults.Count} users matching '{searchText}'";
                StatusLabel.Foreground = new SolidColorBrush(Colors.Green);
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"Search error: {ex.Message}";
                StatusLabel.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        private async void LoadAllUsersButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";
            await LoadAllUsers();
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadAllUsers();
        }

        private void UserListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StartChatButton.IsEnabled = UserListBox.SelectedItem != null;
        }

        private void UserListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (UserListBox.SelectedItem != null)
            {
                StartChatButton_Click(sender, new RoutedEventArgs());
            }
        }

        private async void StartChatButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserListBox.SelectedItem is UserDto selectedUser)
            {
                // Validate token and current user info
                if (string.IsNullOrEmpty(currentToken))
                {
                    MessageBox.Show("No authentication token available. Please login again.", "Authentication Required",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (currentUser == null)
                {
                    MessageBox.Show("Current user information not available. Please refresh the page.", "User Info Required",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    StatusLabel.Text = "Creating conversation...";
                    StatusLabel.Foreground = new SolidColorBrush(Colors.Blue);

                    // Create conversation with selected user
                    var conversationResult = await ChatService.CreateConversationAsync(currentToken, selectedUser._id);

                    if (conversationResult != null && conversationResult.Count > 0)
                    {
                        string conversationId = conversationResult[0];

                        // Open ChatWindow instead of ChattingWindow
                        ChatWindow chatWindow = new ChatWindow(
                            conversationId: conversationId,
                            myId: currentUser._id,
                            myName: currentUser.username,
                            token: currentToken
                        );
                        chatWindow.Owner = this; // Set parent window
                        chatWindow.Show();

                        StatusLabel.Text = "Chat window opened";
                        StatusLabel.Foreground = new SolidColorBrush(Colors.Green);
                    }
                    else
                    {
                        StatusLabel.Text = "Failed to create conversation";
                        StatusLabel.Foreground = new SolidColorBrush(Colors.Red);
                        MessageBox.Show("Failed to create conversation with this user.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    StatusLabel.Text = $"Error: {ex.Message}";
                    StatusLabel.Foreground = new SolidColorBrush(Colors.Red);
                    MessageBox.Show($"Error starting chat: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Show confirmation dialog
            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to logout?",
                "Confirm Logout",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                PerformLogout();
            }
        }

        private void PerformLogout()
        {
            try
            {
                // Clear the stored token
                if (File.Exists("auth.token"))
                {
                    File.Delete("auth.token");
                    StatusLabel.Text = "Logged out successfully";
                    StatusLabel.Foreground = new SolidColorBrush(Colors.Green);
                }

                // Close any open chat windows (child windows)
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is ChatWindow && window.Owner == this)
                    {
                        window.Close();
                    }
                }

                // Open login window
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();

                // Close current window
                this.Close();
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"Logout error: {ex.Message}";
                StatusLabel.Foreground = new SolidColorBrush(Colors.Red);
                MessageBox.Show($"Error during logout: {ex.Message}", "Logout Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
