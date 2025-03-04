using Rebecca.Models;

namespace Rebecca.Services.Interfaces;

/// <summary>
/// 媒体库管理器接口，负责协调扫描流程和管理媒体库状态
/// </summary>
public interface IMediaLibraryManager
{
    /// <summary>
    /// 当前是否正在执行扫描
    /// </summary>
    bool IsScanning { get; }
    
    /// <summary>
    /// 获取媒体库配置
    /// </summary>
    MediaLibraryConfig GetConfig();
    
    /// <summary>
    /// 更新媒体库配置
    /// </summary>
    void SetConfig(MediaLibraryConfig config);
    
    /// <summary>
    /// 启动扫描所有媒体库
    /// </summary>
    Task StartScanAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 取消当前扫描
    /// </summary>
    void CancelScan();
    
    /// <summary>
    /// 获取所有媒体文件
    /// </summary>
    IEnumerable<MediaFile> GetAllMediaFiles();
    
    /// <summary>
    /// 初始化并加载所有媒体文件基本信息
    /// </summary>
    Task InitializeAndLoadFilesAsync();
    
    /// <summary>
    /// 处理单个视频文件
    /// </summary>
    Task ProcessSingleFileAsync(string filePath);
}