using System;

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