using System.IO;

namespace Rebecca.Models;

public class MediaFile
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Path { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Status { get; set; } = "pending";
    public DateTime? LastScanned { get; set; }
    public string? Title { get; set; }
    public int? Year { get; set; }
    public string? PosterPath { get; set; }
    public string? FanartPath { get; set; }
    public string? NfoPath { get; set; }
    public string? ErrorMessage { get; set; }
    public long Size { get; set; }
    
    // 新增属性：指示是否缺失资源
    public bool HasPoster => !string.IsNullOrEmpty(PosterPath) && File.Exists(PosterPath);
    public bool HasFanart => !string.IsNullOrEmpty(FanartPath) && File.Exists(FanartPath);
    public bool HasNfo => !string.IsNullOrEmpty(NfoPath) && File.Exists(NfoPath);
    
    // 检查是否所有元数据都完整
    public bool IsMetadataComplete => HasPoster && HasFanart && HasNfo;

    /// <summary>
    /// 当前正在处理的组件
    /// </summary>
    public ProcessingComponent? ProcessingComponent { get; set; }
}

// 媒体文件状态枚举
public static class MediaFileStatus
{
    public const string Pending = "pending";
    public const string Scanning = "scanning";
    public const string Downloading = "downloading";
    public const string Completed = "completed";
    public const string Error = "error";
}

/// <summary>
/// 正在处理的组件类型
/// </summary>
public enum ProcessingComponent
{
    /// <summary>
    /// 正在识别（扫描阶段）
    /// </summary>
    Scanning,
    
    /// <summary>
    /// 正在处理NFO
    /// </summary>
    Nfo,
    
    /// <summary>
    /// 正在处理海报
    /// </summary>
    Poster,
    
    /// <summary>
    /// 正在处理背景图
    /// </summary>
    Fanart,
    
    /// <summary>
    /// 无正在处理的组件
    /// </summary>
    None
}