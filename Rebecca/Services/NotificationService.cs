using Microsoft.Extensions.Logging;
using Rebecca.Core.WebSockets;
using Rebecca.Models;
using Rebecca.Services.Interfaces;

namespace Rebecca.Services;

/// <summary>
/// 通知服务，负责消息通知
/// </summary>
public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly WebSocketHub _webSocketHub;

    public NotificationService(
        ILogger<NotificationService> logger,
        WebSocketHub webSocketHub)
    {
        _logger = logger;
        _webSocketHub = webSocketHub;
    }

    /// <inheritdoc />
    public async Task NotifyScanStatusAsync(bool isScanning)
    {
        _logger.LogDebug($"发送扫描状态通知: {isScanning}");
        
        try
        {
            await _webSocketHub.BroadcastMessage(MessageType.ScanStatus, new { isScanning });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送扫描状态通知时出错");
        }
    }

    /// <inheritdoc />
    public async Task NotifyFileStatusAsync(MediaFile file)
    {
        if (file == null)
        {
            return;
        }
        
        _logger.LogDebug($"发送文件状态通知: {file.Path}, 状态: {file.Status}");
        
        try
        {
            await _webSocketHub.BroadcastMessage(MessageType.FileStatus, file);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"发送文件状态通知时出错: {file.Path}");
        }
    }

    /// <inheritdoc />
    public async Task NotifyErrorAsync(string message)
    {
        _logger.LogDebug($"发送错误通知: {message}");
        
        try
        {
            await _webSocketHub.BroadcastMessage(MessageType.Error, new { message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送错误通知时出错");
        }
    }
}