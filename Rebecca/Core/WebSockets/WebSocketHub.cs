using Microsoft.Extensions.Logging;
using StdEx.Serialization;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace Rebecca.Core.WebSockets;

public class WebSocketHub
{
    private readonly ILogger<WebSocketHub> _logger;
    private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();

    public WebSocketHub(ILogger<WebSocketHub> logger)
    {
        _logger = logger;
    }

    public void AddSocket(string id, WebSocket socket)
    {
        _sockets.TryAdd(id, socket);
    }

    public async Task RemoveSocket(string id)
    {
        if (_sockets.TryRemove(id, out var socket))
        {
            try
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "连接关闭", CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "关闭 WebSocket 连接时发生错误");
            }
        }
    }

    public async Task BroadcastMessage(string type, object? data = null)
    {
        try
        {
            var message = new WebSocketMessage
            {
                Type = type,
                Data = data
            };

            var messageJson = JsonUtils.Serialize(message, useCamelCase: true);
            var messageBytes = Encoding.UTF8.GetBytes(messageJson);
            var segment = new ArraySegment<byte>(messageBytes);

            var deadSockets = new List<string>();

            foreach (var (id, socket) in _sockets)
            {
                try
                {
                    if (socket.State == WebSocketState.Open)
                    {
                        await socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    else
                    {
                        deadSockets.Add(id);
                    }
                }
                catch (Exception)
                {
                    deadSockets.Add(id);
                }
            }

            foreach (var id in deadSockets)
            {
                await RemoveSocket(id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "广播消息时发生错误");
        }
    }
}