using Microsoft.Extensions.Logging;
using Rebecca.Models;
using Rebecca.Services.Interfaces;
using StdEx.Media.Tmdb.Models;
using StdEx.Serialization;
using System.Collections.Concurrent;
using System.IO;

namespace Rebecca.Services;

/// <summary>
/// 媒体文件仓库，负责管理媒体文件集合
/// </summary>
public class MediaFileRepository : IMediaFileRepository
{
    private readonly ILogger<MediaFileRepository> _logger;
    private readonly IFileSystemService _fileSystemService;
    
    // 存储所有媒体文件的线程安全字典
    private readonly ConcurrentDictionary<string, MediaFile> _mediaFiles = new ConcurrentDictionary<string, MediaFile>();

    public MediaFileRepository(
        ILogger<MediaFileRepository> logger,
        IFileSystemService fileSystemService)
    {
        _logger = logger;
        _fileSystemService = fileSystemService;
    }

    /// <inheritdoc />
    public void AddOrUpdateFile(MediaFile file)
    {
        if (file == null)
        {
            return;
        }
        
        _mediaFiles[file.Path] = file;
        _logger.LogDebug($"添加或更新了媒体文件: {file.Path}, 状态: {file.Status}");
    }

    /// <inheritdoc />
    public MediaFile? GetFile(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }
        
        _mediaFiles.TryGetValue(path, out var file);
        return file;
    }

    /// <inheritdoc />
    public IEnumerable<MediaFile> GetAllFiles()
    {
        return _mediaFiles.Values.ToList();
    }

    /// <inheritdoc />
    public bool FileExists(string path)
    {
        return !string.IsNullOrEmpty(path) && _mediaFiles.ContainsKey(path);
    }

    /// <inheritdoc />
    public MediaFile CreateBasicMediaFileInfo(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        
        // 获取相关路径
        string posterPath = _fileSystemService.GetPosterPath(filePath);
        string fanartPath = _fileSystemService.GetFanartPath(filePath);
        string nfoPath = _fileSystemService.GetNfoPath(filePath);
        
        var mediaFile = new MediaFile
        {
            Path = filePath,
            FileName = Path.GetFileName(filePath),
            Status = MediaFileStatus.Pending,
            Size = fileInfo.Length,
            PosterPath = File.Exists(posterPath) ? posterPath : null,
            FanartPath = File.Exists(fanartPath) ? fanartPath : null,
            NfoPath = File.Exists(nfoPath) ? nfoPath : null
        };
        
        // 如果NFO文件存在，尝试从中读取电影信息
        if (File.Exists(nfoPath))
        {
            try
            {
                var movieNfo = XmlUtils.Deserialize<MovieNfo>(File.ReadAllText(nfoPath));
                mediaFile.Title = movieNfo.Title;
                mediaFile.Year = movieNfo.Year;
                mediaFile.Status = MediaFileStatus.Completed;
                mediaFile.LastScanned = File.GetLastWriteTime(nfoPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"读取现有NFO文件失败: {nfoPath}");
            }
        }
        
        return mediaFile;
    }

    /// <inheritdoc />
    public void CheckMetadataFiles(MediaFile mediaFile)
    {
        if (mediaFile == null) return;
        
        // 更新元数据文件路径和状态
        string posterPath = _fileSystemService.GetPosterPath(mediaFile.Path);
        string fanartPath = _fileSystemService.GetFanartPath(mediaFile.Path);
        string nfoPath = _fileSystemService.GetNfoPath(mediaFile.Path);
        
        mediaFile.PosterPath = File.Exists(posterPath) ? posterPath : null;
        mediaFile.FanartPath = File.Exists(fanartPath) ? fanartPath : null;
        mediaFile.NfoPath = File.Exists(nfoPath) ? nfoPath : null;
        
        // 如果所有元数据都存在但状态不是完成，则更新状态
        if (mediaFile.HasPoster && mediaFile.HasFanart && mediaFile.HasNfo && 
            mediaFile.Status != MediaFileStatus.Completed)
        {
            mediaFile.Status = MediaFileStatus.Completed;
            _logger.LogInformation($"媒体文件 {mediaFile.Path} 的元数据已完整，更新状态为完成");
        }
    }
}