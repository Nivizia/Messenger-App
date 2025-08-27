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

                partnerName = other["data"]?[0]?.ToString() ?? "Người khác";

                Dispatcher.Invoke(() =>
                {
                    UserInfoText.Text = $"Bạn: {myName} — Đối phương: {partnerName}";
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
            string text = MessageInput.Text.Trim();
            if (string.IsNullOrEmpty(text)) return;

            AppendMessage(myName, text, true);

            var bytes = Encoding.UTF8.GetBytes(text);
            await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);

            MessageInput.Text = "";
        }

        private void AppendMessage(string sender, string text, bool isMine)
        {
            var msgBlock = new TextBlock
            {
                Text = $"{sender}: {text}",
                Margin = new Thickness(5),
                HorizontalAlignment = isMine ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                Background = isMine ? System.Windows.Media.Brushes.LightBlue : System.Windows.Media.Brushes.LightGray,
                Padding = new Thickness(8),
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 300
            };

            MessagesPanel.Children.Add(msgBlock);
        }
    }
}
