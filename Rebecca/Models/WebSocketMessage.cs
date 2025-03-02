namespace Rebecca.Models;

public class WebSocketMessage
{
    public string Type { get; set; } = string.Empty;
    public object? Data { get; set; }
}

public static class WebSocketEventType
{
    public const string ScanStatus = "scanStatus";
    public const string FileStatus = "fileStatus";
    public const string Error = "error";
}