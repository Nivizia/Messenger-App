// MessengerUI/Services/WsChatClient.cs
using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MessengerUI.Services
{
    public sealed class WsChatClient : IAsyncDisposable
    {
        private readonly string _wsBaseUrl;   // ví dụ: "wss://api.mmb.io.vn/py/api/chatbox/ws"
        private readonly string _token;
        private ClientWebSocket? _ws;
        private CancellationTokenSource? _cts;

        // sự kiện bắn ra khi nhận được text frame
        public event Action<string>? OnText;

        public WsChatClient(string wsBaseUrl, string token)
        {
            _wsBaseUrl = wsBaseUrl ?? throw new ArgumentNullException(nameof(wsBaseUrl));
            _token = token ?? throw new ArgumentNullException(nameof(token));
        }

        // Kết nối và join room
        public async Task ConnectAsync(string conversationId)
        {
            _cts = new CancellationTokenSource();
            _ws = new ClientWebSocket();
            _ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(30);

            // đa số backend nhận token qua query
            var url = $"{_wsBaseUrl}?token={Uri.EscapeDataString(_token)}&conversation_id={Uri.EscapeDataString(conversationId)}";
            await _ws.ConnectAsync(new Uri(url), _cts.Token);

            // bắt đầu nhận
            _ = Task.Run(() => ReceiveLoopAsync(_cts.Token));

            // nhiều server yêu cầu gửi gói join
            var join = JsonSerializer.Serialize(new { action = "join", conversation_id = conversationId });
            await SendRawAsync(join);
        }

        // Gửi tin nhắn (payload có thể chỉnh theo BE)
        public async Task SendMessageAsync(string conversationId, string content)
        {
            var payload = JsonSerializer.Serialize(new
            {
                action = "send_message",
                conversation_id = conversationId,
                content = content
            });
            await SendRawAsync(payload);
        }

        private async Task SendRawAsync(string text)
        {
            if (_ws == null || _ws.State != WebSocketState.Open)
                throw new InvalidOperationException("WS not connected");

            var bytes = Encoding.UTF8.GetBytes(text);
            await _ws.SendAsync(bytes, WebSocketMessageType.Text, true, _cts!.Token);
        }

        private async Task ReceiveLoopAsync(CancellationToken ct)
        {
            if (_ws == null) return;
            var buf = new byte[8192];

            try
            {
                while (!ct.IsCancellationRequested && _ws.State == WebSocketState.Open)
                {
                    var sb = new StringBuilder();
                    WebSocketReceiveResult result;
                    do
                    {
                        var seg = new ArraySegment<byte>(buf);
                        result = await _ws.ReceiveAsync(seg, ct);
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None);
                            return;
                        }
                        sb.Append(Encoding.UTF8.GetString(buf, 0, result.Count));
                    }
                    while (!result.EndOfMessage);

                    OnText?.Invoke(sb.ToString());
                }
            }
            catch (Exception ex)
            {
                OnText?.Invoke("[WS error] " + ex.Message);
            }
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                _cts?.Cancel();
                if (_ws is { State: WebSocketState.Open })
                    await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "dispose", CancellationToken.None);
                _ws?.Dispose();
            }
            catch { /* ignore */ }
        }
    }
}
