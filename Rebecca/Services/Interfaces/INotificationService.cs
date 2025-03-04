using Rebecca.Models;

namespace Rebecca.Services.Interfaces;

/// <summary>
/// 通知服务接口，负责消息通知
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// 发送扫描状态通知
    /// </summary>
    /// <param name="isScanning">是否正在扫描</param>
    Task NotifyScanStatusAsync(bool isScanning);
    
    /// <summary>
    /// 发送文件状态更新通知
    /// </summary>
    /// <param name="file">媒体文件</param>
    Task NotifyFileStatusAsync(MediaFile file);
    
    /// <summary>
    /// 发送错误消息通知
    /// </summary>
    /// <param name="message">错误消息</param>
    Task NotifyErrorAsync(string message);
}