using Microsoft.AspNetCore.Http;
using System.Net.WebSockets;

namespace Rebecca.Core.WebSockets;

public class WebSocketService
{
    private readonly WebSocketHub _webSocketHub;

    public WebSocketService(WebSocketHub webSocketHub)
    {
        _webSocketHub = webSocketHub;
    }

    public async Task HandleWebSocket(HttpContext context)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            var socketId = Guid.NewGuid().ToString();

            try
            {
                _webSocketHub.AddSocket(socketId, webSocket);

                // 保持连接直到客户端断开
                var buffer = new byte[1024 * 4];
                while (webSocket.State == WebSocketState.Open)
                {
                    try
                    {
                        var result = await webSocket.ReceiveAsync(
                            new ArraySegment<byte>(buffer), CancellationToken.None);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                                "Closing as per client request", CancellationToken.None);
                            break;
                        }
                    }
                    catch (WebSocketException)
                    {
                        break;
                    }
                }
            }
            finally
            {
                await _webSocketHub.RemoveSocket(socketId);
            }
        }
        else
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("不是 WebSocket 请求");
        }
    }
}