using ApiFetcher;
using MessengerUI.Services;  
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MessengerUI
{
    public partial class ChattingWindow : Window
    {
        private readonly string _token;
        private readonly string _conversationId;

        // ⚠️ Điền đúng URL WS thật khi bạn có (tạm để chờ BE):
        private const string WsBaseUrl = "wss://api.mmb.io.vn/py/api/chatbox/ws";

        private WsChatClient? _ws;

        public ChattingWindow(string token, string conversationId, string? displayName = null)
        {
            InitializeComponent();
            _token = token;
            _conversationId = conversationId;

            if (!string.IsNullOrWhiteSpace(displayName))
            {
                Title = $"Chat with {displayName}";
                UserInfoText.Text = displayName;
            }

            Loaded += async (_, __) =>
            {
                await LoadMessagesAsync();      // đọc lịch sử (REST)
                await ConnectWsAsync();         // nối WS để gửi/nhận
            };
            Closed += async (_, __) => { if (_ws != null) await _ws.DisposeAsync(); };
        }

        private async Task ConnectWsAsync()
        {
            try
            {
                _ws = new WsChatClient(WsBaseUrl, _token);
                _ws.OnText += text =>
                {
                    // cố thử parse JSON, có field content thì hiển thị đẹp; nếu không thì show raw
                    try
                    {
                        var doc = JsonDocument.Parse(text);
                        var content = doc.RootElement.TryGetProperty("content", out var c)
                                      ? c.GetString() ?? text
                                      : text;
                        Dispatcher.Invoke(() =>
                        {
                            MessagesPanel.Children.Add(CreateBubble($"{DateTime.Now:HH:mm}  {content}"));
                            SvMessages.ScrollToEnd();
                        });
                    }
                    catch
                    {
                        Dispatcher.Invoke(() =>
                        {
                            MessagesPanel.Children.Add(CreateBubble("[WS] " + text));
                            SvMessages.ScrollToEnd();
                        });
                    }
                };

                await _ws.ConnectAsync(_conversationId);
                MessagesPanel.Children.Add(CreateBubble("[WS] Connected"));
                SvMessages.ScrollToEnd();
            }
            catch (Exception ex)
            {
                MessagesPanel.Children.Add(CreateBubble("[WS] Connect failed: " + ex.Message));
            }
        }

        private async Task LoadMessagesAsync()
        {
            try
            {
                MessagesPanel.Children.Clear();
                var msgs = await ChatService.GetMessagesAsync(_token, _conversationId, 0, 50) ?? new List<MessageDto>();
                foreach (var m in msgs)
                    MessagesPanel.Children.Add(CreateBubble($"{m.created_at:HH:mm}  {m.content}"));

                await Task.Delay(10);
                SvMessages.ScrollToEnd();
            }
            catch (Exception ex)
            {
                MessagesPanel.Children.Add(CreateBubble("[REST] load failed: " + ex.Message));
            }
        }

        private bool _wsReady = false;

        

        private async void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            var text = (MessageInput.Text ?? "").Trim();
            if (text.Length == 0) return;

            try
            {
                if (!_wsReady || _ws == null)                                 // ✅
                {
                    MessageBox.Show("WS chưa kết nối. Kiểm tra lại WS URL/cách auth.");
                    return;
                }

                await _ws.SendMessageAsync(_conversationId, text);

                MessagesPanel.Children.Add(CreateBubble($"{DateTime.Now:HH:mm}  {text}"));
                MessageInput.Clear();
                SvMessages.ScrollToEnd();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Send failed: {ex.Message}");
            }
        }


        private System.Windows.Controls.Border CreateBubble(string text)
        {
            return new System.Windows.Controls.Border
            {
                Margin = new Thickness(6, 4, 6, 4),
                Padding = new Thickness(10, 6, 10, 6),
                CornerRadius = new CornerRadius(10),
                Background = new SolidColorBrush(Color.FromRgb(0x22, 0x2A, 0x44)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x1B, 0x21, 0x40)),
                BorderThickness = new Thickness(1),
                Child = new System.Windows.Controls.TextBlock
                {
                    Text = text,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = Brushes.White
                }
            };
        }

       
    }
}
