using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MessengerUI
{
    public partial class ChatWindow : Window
    {
        private readonly string API_BASE = "https://api.mmb.io.vn/py";
        private readonly string conversationId;
        private readonly string myId;
        private readonly string myName;
        private readonly string token;

        private string partnerName = "Người khác";
        private ClientWebSocket ws;

        public ChatWindow(string conversationId, string myId, string myName, string token)
        {
            InitializeComponent();
            this.conversationId = conversationId;
            this.myId = myId;
            this.myName = myName;
            this.token = token;

            _ = InitChat();
        }

        private async Task InitChat()
        {
            await LoadPartnerInfo();
            await LoadMessages();
            await StartWebSocket();
        }

        private async Task LoadPartnerInfo()
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                // Lấy danh sách phòng của mình
                var res = await client.GetStringAsync($"{API_BASE}/api/chatbox/conversation/me");
                var data = JObject.Parse(res);

                JArray rooms = (JArray)data["data"][0];
                var room = rooms.First(r => r["_id"]?.ToString() == conversationId);

                string otherId = myId == room["participant_1"]?.ToString()
                    ? room["participant_2"]?.ToString()
                    : room["participant_1"]?.ToString();

                // Lấy thông tin đối phương
                var res2 = await client.GetStringAsync($"{API_BASE}/api/chatboxconversation/user/{otherId}");
                var other = JObject.Parse(res2);

                partnerName = other["data"]?[0]?.ToString() ?? "Unknown User";

                Dispatcher.Invoke(() =>
                {
                    UserInfoText.Text = partnerName;
                });
            }
        }

        private async Task LoadMessages()
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                var res = await client.GetStringAsync($"{API_BASE}/api/chatbox/messages?id={conversationId}&skip=0&limit=10");
                var data = JObject.Parse(res);

                foreach (var msg in data["data"][0])
                {
                    string senderId = msg["sender_id"]?.ToString();
                    string content = msg["content"]?.ToString();
                    bool isMine = senderId == myId;
                    string sender = isMine ? myName : partnerName;

                    Dispatcher.Invoke(() => AppendMessage(sender, content, isMine));
                }
            }
        }

        private async Task StartWebSocket()
        {
            ws = new ClientWebSocket();
            var uri = new Uri($"wss://api.mmb.io.vn/py/websocket/chatbox/{conversationId}/{myId}?token={token}");
            await ws.ConnectAsync(uri, CancellationToken.None);

            _ = Task.Run(ReceiveMessages);
        }

        private async Task ReceiveMessages()
        {
            var buffer = new byte[4096];
            while (ws.State == WebSocketState.Open)
            {
                var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                }
                else
                {
                    string json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var msg = JObject.Parse(json);

                    string senderId = msg["sender_id"]?.ToString();
                    string content = msg["content"]?.ToString();

                    if (senderId != myId)
                    {
                        Dispatcher.Invoke(() => AppendMessage(partnerName, content, false));
                    }
                }
            }
        }

        private async void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            await SendMessage();
        }

        private void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                e.Handled = true; // Prevent the Enter from being processed by the TextBox
                _ = SendMessage(); // Send the message
            }
        }

        private async Task SendMessage()
        {
            string text = MessageInput.Text.Trim();
            if (string.IsNullOrEmpty(text)) return;

            AppendMessage(myName, text, true);

            var bytes = Encoding.UTF8.GetBytes(text);
            await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);

            MessageInput.Text = "";
            MessageInput.Focus(); // Keep focus on input after sending

            // Auto-scroll to bottom
            MessagesScrollViewer.ScrollToEnd();
        }

        private void AppendMessage(string sender, string text, bool isMine)
        {
            // Create message container
            var messageContainer = new Grid
            {
                Margin = new Thickness(0, 3, 0, 3)
            };

            messageContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            messageContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Create message bubble
            var messageBorder = new Border
            {
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(10, 6, 10, 6),
                MaxWidth = 350,
                HorizontalAlignment = isMine ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                Background = isMine ?
                    new SolidColorBrush(Color.FromRgb(255, 198, 92)) : // #FFC65C - App accent color for sent messages
                    new SolidColorBrush(Color.FromRgb(108, 102, 153)), // #6C6699 - App button color for received messages
                Margin = isMine ? new Thickness(40, 0, 0, 0) : new Thickness(0, 0, 40, 0)
            };

            // Create message content stack
            var messageStack = new StackPanel();

            // Sender name (only show for received messages)
            if (!isMine)
            {
                var senderText = new TextBlock
                {
                    Text = sender,
                    FontSize = 11,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White,
                    Margin = new Thickness(0, 0, 0, 2)
                };
                messageStack.Children.Add(senderText);
            }

            // Message text
            var messageText = new TextBlock
            {
                Text = text,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14,
                FontWeight = FontWeights.Normal,
                Foreground = isMine ?
                    new SolidColorBrush(Color.FromRgb(39, 37, 55)) : // Dark text on light background (sent)
                    Brushes.White, // White text on dark background (received)
                LineHeight = 16
            };
            messageStack.Children.Add(messageText);

            messageBorder.Child = messageStack;

            // Position the message bubble
            Grid.SetColumn(messageBorder, isMine ? 1 : 0);
            messageContainer.Children.Add(messageBorder);

            MessagesPanel.Children.Add(messageContainer);

            // Auto-scroll to bottom
            Dispatcher.BeginInvoke(new Action(() =>
            {
                MessagesScrollViewer.ScrollToEnd();
            }), System.Windows.Threading.DispatcherPriority.Background);
        }
    }
}
