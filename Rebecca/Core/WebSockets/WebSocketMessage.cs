namespace Rebecca.Core.WebSockets;

public class WebSocketMessage
{
    public string Type { get; set; } = string.Empty;
    public object? Data { get; set; }
}
