using Microsoft.Extensions.Logging;
using Rebecca.Services.Interfaces;
using StdEx.Media.Tmdb;
using StdEx.Media.Tmdb.Models;
using StdEx.Serialization;
using System.IO;

namespace Rebecca.Services;

/// <summary>
/// 元数据服务，负责处理媒体元数据
/// </summary>
public class MetadataService : IMetadataService
{
    private readonly ILogger<MetadataService> _logger;
    private readonly ITmdbSettingsService _tmdbSettingsService;
    private readonly IDownloadService _downloadService;

    public MetadataService(
        ILogger<MetadataService> logger,
        ITmdbSettingsService tmdbSettingsService,
        IDownloadService downloadService)
    {
        _logger = logger;
        _tmdbSettingsService = tmdbSettingsService;
        _downloadService = downloadService;
    }

    /// <inheritdoc />
    public async Task<MovieNfo> GetMovieMetadataAsync(string movieName, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"获取电影元数据: {movieName}");
        
        try
        {
            var config = await _tmdbSettingsService.GetConfigAsync();
            var tmdbUtils = new TmdbUtils(config);
            
            var movieNfo = await tmdbUtils.GetMovieNfo(movieName);
            _logger.LogInformation($"成功获取电影元数据: {movieName}, 标题: {movieNfo.Title}");
            
            return movieNfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"获取电影元数据时出错: {movieName}");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task DownloadPosterAsync(string url, string localPath, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"下载电影海报: {url}");
        
        if (File.Exists(localPath))
        {
            _logger.LogInformation($"海报已存在，跳过下载: {localPath}");
            return;
        }
        
        try
        {
            await _downloadService.DownloadToFileAsync(url, localPath, cancellationToken);
            _logger.LogInformation($"海报下载成功: {localPath}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"下载海报时出错: {url}");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task DownloadFanartAsync(string url, string localPath, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"下载电影背景图: {url}");
        
        if (File.Exists(localPath))
        {
            _logger.LogInformation($"背景图已存在，跳过下载: {localPath}");
            return;
        }
        
        try
        {
            await _downloadService.DownloadToFileAsync(url, localPath, cancellationToken);
            _logger.LogInformation($"背景图下载成功: {localPath}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"下载背景图时出错: {url}");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task CreateNfoFileAsync(MovieNfo metadata, string nfoPath, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"创建NFO文件: {nfoPath}");
        
        try
        {
            // 确保目标目录存在
            var directory = Path.GetDirectoryName(nfoPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            string nfoXml = XmlUtils.Serialize(metadata);
            await File.WriteAllTextAsync(nfoPath, nfoXml, cancellationToken);
            
            _logger.LogInformation($"NFO文件创建成功: {nfoPath}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"创建NFO文件时出错: {nfoPath}");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<MovieNfo?> ReadNfoFileAsync(string nfoPath)
    {
        _logger.LogInformation($"读取NFO文件: {nfoPath}");
        
        if (!File.Exists(nfoPath))
        {
            _logger.LogWarning($"NFO文件不存在: {nfoPath}");
            return null;
        }
        
        try
        {
            string nfoContent = await File.ReadAllTextAsync(nfoPath);
            var movieNfo = XmlUtils.Deserialize<MovieNfo>(nfoContent);
            
            _logger.LogInformation($"成功读取NFO文件: {nfoPath}, 标题: {movieNfo.Title}");
            return movieNfo;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"读取NFO文件时出错: {nfoPath}");
            return null;
        }
    }
}