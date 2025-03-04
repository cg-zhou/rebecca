using Microsoft.Extensions.Logging;
using Rebecca.Services.Interfaces;
using System.IO;

namespace Rebecca.Services;

/// <summary>
/// 文件系统服务，处理文件系统相关操作
/// </summary>
public class FileSystemService : IFileSystemService
{
    private readonly ILogger<FileSystemService> _logger;
    
    // 支持的视频文件扩展名
    private static readonly string[] VideoExtensions =
    [
        ".mp4", ".mkv", ".avi", ".mov",
        ".wmv", ".flv", ".m4v", ".rmvb"
    ];

    public FileSystemService(ILogger<FileSystemService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public IEnumerable<string> CollectVideoFiles(string folderPath)
    {
        _logger.LogInformation($"从目录收集视频文件: {folderPath}");
        try
        {
            return Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories)
                .Where(f => IsVideoFile(f));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"从目录收集文件时出错: {folderPath}");
            return Enumerable.Empty<string>();
        }
    }

    /// <inheritdoc />
    public bool IsVideoFile(string filePath)
    {
        return VideoExtensions.Contains(Path.GetExtension(filePath).ToLowerInvariant());
    }

    /// <inheritdoc />
    public string GetMovieName(string fileName)
    {
        return fileName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
    }

    /// <inheritdoc />
    public string GetPosterPath(string videoPath)
    {
        string directory = Path.GetDirectoryName(videoPath) ?? string.Empty;
        string baseName = Path.GetFileNameWithoutExtension(videoPath);
        return Path.Combine(directory, $"{baseName}-poster.jpg");
    }

    /// <inheritdoc />
    public string GetFanartPath(string videoPath)
    {
        string directory = Path.GetDirectoryName(videoPath) ?? string.Empty;
        string baseName = Path.GetFileNameWithoutExtension(videoPath);
        return Path.Combine(directory, $"{baseName}-fanart.jpg");
    }

    /// <inheritdoc />
    public string GetNfoPath(string videoPath)
    {
        string directory = Path.GetDirectoryName(videoPath) ?? string.Empty;
        string baseName = Path.GetFileNameWithoutExtension(videoPath);
        return Path.Combine(directory, $"{baseName}.nfo");
    }
}