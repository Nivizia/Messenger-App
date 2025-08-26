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

namespace MessengerUI
{
    /// <summary>
    /// Interaction logic for ChattingWindow.xaml
    /// </summary>
    public partial class ChattingWindow : Window
    {
        public string ConversationId { get; set; } = string.Empty;
        public string CurrentToken { get; set; } = string.Empty;
        public string ChatPartnerName { get; set; } = string.Empty;

        private List<MessageDto> messages = new List<MessageDto>();

        public ChattingWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ChattingUserName.Text = $"Chat with: {ChatPartnerName}";
            await LoadMessages();
        }

        private async Task LoadMessages()
        {
            try
            {
                StatusLabel.Text = "Loading messages...";
                StatusLabel.Foreground = new SolidColorBrush(Colors.Blue);

                if (string.IsNullOrEmpty(ConversationId))
                {
                    StatusLabel.Text = "No conversation ID provided";
                    StatusLabel.Foreground = new SolidColorBrush(Colors.Red);
                    return;
                }

                var loadedMessages = await ChatService.GetMessagesAsync(CurrentToken, ConversationId);

                if (loadedMessages != null)
                {
                    messages = loadedMessages;
                    DisplayMessages();
                    StatusLabel.Text = $"Loaded {messages.Count} messages";
                    StatusLabel.Foreground = new SolidColorBrush(Colors.Green);
                }
                else
                {
                    messages = new List<MessageDto>();
                    DisplayMessages();
                    StatusLabel.Text = "No messages found";
                    StatusLabel.Foreground = new SolidColorBrush(Colors.Orange);
                }

                // Scroll to bottom
                ScrollToBottom();
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"Error loading messages: {ex.Message}";
                StatusLabel.Foreground = new SolidColorBrush(Colors.Red);
                MessageBox.Show($"Failed to load messages: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisplayMessages()
        {
            ChatHistoryPanel.Children.Clear();

            foreach (var message in messages.OrderBy(m => m.created_at))
            {
                // Create message bubble
                Border messageBubble = new Border
                {
                    Margin = new Thickness(5),
                    Padding = new Thickness(10),
                    CornerRadius = new CornerRadius(10),
                    MaxWidth = 400
                };

                // Determine if message is from current user (this is a simplified check)
                bool isOwnMessage = message.sender_id != "other_user"; // TODO: Implement proper user ID checking

                if (isOwnMessage)
                {
                    // Own messages - right aligned, blue background
                    messageBubble.Background = new SolidColorBrush(Colors.LightBlue);
                    messageBubble.HorizontalAlignment = HorizontalAlignment.Right;
                }
                else
                {
                    // Other messages - left aligned, light gray background
                    messageBubble.Background = new SolidColorBrush(Colors.LightGray);
                    messageBubble.HorizontalAlignment = HorizontalAlignment.Left;
                }

                // Create message content
                StackPanel messageContent = new StackPanel();

                TextBlock messageText = new TextBlock
                {
                    Text = message.content,
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 14
                };

                TextBlock timeText = new TextBlock
                {
                    Text = message.created_at.ToString("HH:mm"),
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Colors.Gray),
                    HorizontalAlignment = isOwnMessage ? HorizontalAlignment.Right : HorizontalAlignment.Left
                };

                messageContent.Children.Add(messageText);
                messageContent.Children.Add(timeText);
                messageBubble.Child = messageContent;

                ChatHistoryPanel.Children.Add(messageBubble);
            }
        }

        private void ScrollToBottom()
        {
            ChatScrollViewer.ScrollToEnd();
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            await SendMessage();
        }

        private async void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(MessageInput.Text))
            {
                await SendMessage();
            }
        }

        private async Task SendMessage()
        {
            string messageText = MessageInput.Text.Trim();

            if (string.IsNullOrEmpty(messageText))
            {
                return;
            }

            try
            {
                StatusLabel.Text = "Sending message...";
                StatusLabel.Foreground = new SolidColorBrush(Colors.Blue);

                // Disable input while sending
                MessageInput.IsEnabled = false;
                SendButton.IsEnabled = false;

                // Send message using the placeholder API
                bool success = await ChatService.SendMessageAsync(CurrentToken, ConversationId, messageText);

                if (success)
                {
                    // Clear input
                    MessageInput.Text = "";

                    // Add message to local display immediately (optimistic update)
                    var newMessage = new MessageDto
                    {
                        _id = Guid.NewGuid().ToString(),
                        conversation_id = ConversationId,
                        sender_id = "current_user", // TODO: Get actual current user ID
                        content = messageText,
                        created_at = DateTime.Now
                    };

                    messages.Add(newMessage);
                    DisplayMessages();
                    ScrollToBottom();

                    StatusLabel.Text = "Message sent successfully";
                    StatusLabel.Foreground = new SolidColorBrush(Colors.Green);
                }
                else
                {
                    StatusLabel.Text = "Failed to send message";
                    StatusLabel.Foreground = new SolidColorBrush(Colors.Red);
                    MessageBox.Show("Failed to send message. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"Error sending message: {ex.Message}";
                StatusLabel.Foreground = new SolidColorBrush(Colors.Red);
                MessageBox.Show($"Error sending message: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Re-enable input
                MessageInput.IsEnabled = true;
                SendButton.IsEnabled = true;
                MessageInput.Focus();
            }
        }

        private async void RefreshMessagesButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadMessages();
        }
    }
}
