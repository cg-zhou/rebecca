using Microsoft.Extensions.Logging;
using Rebecca.Models;
using StdEx.Media.Tmdb;
using StdEx.Serialization;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Security.Authentication;

namespace Rebecca.Services;

public class MediaLibraryService : IDisposable
{
    private readonly ILogger<MediaLibraryService> _logger;
    private readonly ITmdbSettingsService _tmdbSettingsService;
    private readonly MediaLibraryConfigService _configService;

    // 存储所有媒体文件的字典
    private readonly ConcurrentDictionary<string, MediaFile> _mediaFiles = new ConcurrentDictionary<string, MediaFile>();

    // 支持的视频文件扩展名
    private static readonly string[] VideoExtensions =
    [
        ".mp4", ".mkv", ".avi", ".mov",
        ".wmv", ".flv", ".m4v", ".rmvb"
    ];

    // 用于取消当前扫描任务
    private CancellationTokenSource? _scanCancellationTokenSource;

    private readonly HttpClient _httpClient;
    private const int MaxRetries = 3;

    public bool IsScanning { get; private set; } = false;

    public MediaLibraryService(
        ILogger<MediaLibraryService> logger, 
        ITmdbSettingsService tmdbSettingsService,
        MediaLibraryConfigService configService)
    {
        _logger = logger;
        _tmdbSettingsService = tmdbSettingsService;
        _configService = configService;

        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true,
            SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
            CheckCertificateRevocationList = false
        };
        _httpClient = new HttpClient(handler);
        _httpClient.Timeout = TimeSpan.FromMinutes(5);
    }

    /// <summary>
    /// 获取配置
    /// </summary>
    public MediaLibraryConfig GetConfig()
    {
        return _configService.GetConfig();
    }

    /// <summary>
    /// 更新配置
    /// </summary>
    public void SetConfig(MediaLibraryConfig config)
    {
        _configService.SetConfig(config);
    }

    /// <summary>
    /// 扫描所有媒体库
    /// </summary>
    public async Task StartScanAsync(CancellationToken cancellationToken = default)
    {
        if (IsScanning)
        {
            _logger.LogInformation("A scan is already in progress");
            return;
        }

        try
        {
            IsScanning = true;
            _scanCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            var config = GetConfig();
            var filesToProcess = new List<string>();

            // 第一阶段：快速扫描收集所有视频文件
            foreach (var path in config.LibraryPaths)
            {
                if (_scanCancellationTokenSource.Token.IsCancellationRequested)
                {
                    break;
                }

                if (Directory.Exists(path))
                {
                    var files = CollectVideoFiles(path);
                    filesToProcess.AddRange(files);

                    // 快速添加到媒体文件列表，仅基本信息
                    foreach (var file in files)
                    {
                        if (!_mediaFiles.ContainsKey(file))
                        {
                            _mediaFiles[file] = new MediaFile
                            {
                                Path = file,
                                FileName = Path.GetFileName(file),
                                Status = MediaFileStatus.Pending
                            };
                        }
                    }
                }
                else
                {
                    _logger.LogWarning($"Library path does not exist: {path}");
                }
            }

            // 第二阶段：处理每个文件的详细信息
            foreach (var file in filesToProcess)
            {
                if (_scanCancellationTokenSource.Token.IsCancellationRequested)
                {
                    break;
                }

                await ProcessVideoFileAsync(file, _scanCancellationTokenSource.Token);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Scan operation was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while scanning libraries");
        }
        finally
        {
            IsScanning = false;
            _scanCancellationTokenSource = null;
        }
    }

    private IEnumerable<string> CollectVideoFiles(string folderPath)
    {
        _logger.LogInformation($"Collecting video files from: {folderPath}");
        try
        {
            return Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories)
                .Where(f => VideoExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error collecting files from folder: {folderPath}");
            return Enumerable.Empty<string>();
        }
    }

    /// <summary>
    /// 取消当前扫描
    /// </summary>
    public void CancelScan()
    {
        _scanCancellationTokenSource?.Cancel();
    }

    /// <summary>
    /// 扫描单个文件夹
    /// </summary>
    private async Task ScanFolderAsync(string folderPath, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Scanning folder: {folderPath}");

        try
        {
            // 获取所有视频文件
            var videoFiles = Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories)
                .Where(f => VideoExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()));

            foreach (var file in videoFiles)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                await ProcessVideoFileAsync(file, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error scanning folder: {folderPath}");
        }
    }

    /// <summary>
    /// 处理单个视频文件
    /// </summary>
    private async Task ProcessVideoFileAsync(string filePath, CancellationToken cancellationToken)
    {
        // 如果文件已经在字典中并且状态为已完成，则跳过
        if (_mediaFiles.TryGetValue(filePath, out var existingFile) && existingFile.Status == MediaFileStatus.Completed)
        {
            // 如果文件的修改时间没变，则跳过
            var fileInfo = new FileInfo(filePath);
            if (existingFile.LastScanned.HasValue && fileInfo.LastWriteTime <= existingFile.LastScanned.Value)
            {
                return;
            }
        }

        // 创建或更新媒体文件信息
        var mediaFile = existingFile ?? new MediaFile
        {
            Path = filePath,
            FileName = Path.GetFileName(filePath)
        };

        try
        {
            mediaFile.Status = MediaFileStatus.Scanning;
            _mediaFiles[filePath] = mediaFile;

            // 从文件名猜测电影名称
            string movieName = GetMovieName(Path.GetFileNameWithoutExtension(filePath));

            // 更新状态为下载元数据中
            mediaFile.Status = MediaFileStatus.Downloading;
            _mediaFiles[filePath] = mediaFile;

            // 获取TMDB配置并创建TmdbUtils实例
            var config = await _tmdbSettingsService.GetConfigAsync();
            var tmdbUtils = new TmdbUtils(config);

            // 从TMDB获取电影信息
            var movieNfo = await tmdbUtils.GetMovieNfo(movieName);

            string directory = Path.GetDirectoryName(filePath) ?? string.Empty;
            string baseName = Path.GetFileNameWithoutExtension(filePath);

            // 设置本地图片路径
            string posterPath = Path.Combine(directory, $"{baseName}-poster.jpg");
            string fanartPath = Path.Combine(directory, $"{baseName}-fanart.jpg");
            string nfoPath = Path.Combine(directory, $"{baseName}.nfo");

            // 下载海报图片
            await DownloadImageAsync(movieNfo.Art.Poster, posterPath, cancellationToken);

            // 下载背景图片
            await DownloadImageAsync(movieNfo.Art.Fanart, fanartPath, cancellationToken);

            // 更新本地路径
            movieNfo.Art.LocalPoster = posterPath;
            movieNfo.Art.LocalFanart = fanartPath;

            // 保存NFO文件
            string nfoXml = XmlUtils.Serialize(movieNfo);
            File.WriteAllText(nfoPath, nfoXml);

            // 更新媒体文件信息
            mediaFile.Status = MediaFileStatus.Completed;
            mediaFile.Title = movieNfo.Title;
            mediaFile.Year = movieNfo.Year;
            mediaFile.PosterPath = posterPath;
            mediaFile.FanartPath = fanartPath;
            mediaFile.NfoPath = nfoPath;
            mediaFile.LastScanned = DateTime.Now;
            _mediaFiles[filePath] = mediaFile;

            _logger.LogInformation($"Successfully processed file: {filePath}");
        }
        catch (Exception ex)
        {
            mediaFile.Status = MediaFileStatus.Error;
            mediaFile.ErrorMessage = ex.Message;
            _mediaFiles[filePath] = mediaFile;
            _logger.LogError(ex, $"Error processing file: {filePath}");
        }
    }

    /// <summary>
    /// 从文件名猜测电影名称
    /// </summary>
    private string GetMovieName(string fileName)
    {
        return fileName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
    }

    /// <summary>
    /// 下载图片
    /// </summary>
    private async Task DownloadImageAsync(string url, string localPath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(url))
        {
            return;
        }

        for (int retry = 0; retry < MaxRetries; retry++)
        {
            try
            {
                using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                response.EnsureSuccessStatusCode();

                // 确保目标目录存在
                var directory = Path.GetDirectoryName(localPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var fileStream = File.Create(localPath);
                await stream.CopyToAsync(fileStream, cancellationToken);
                return; // 下载成功，退出重试循环
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is IOException)
            {
                if (retry == MaxRetries - 1)
                {
                    _logger.LogError(ex, $"Failed to download image after {MaxRetries} attempts. URL: {url}");
                    throw;
                }
                _logger.LogWarning($"Download attempt {retry + 1} failed, retrying... URL: {url}");
                await Task.Delay(TimeSpan.FromSeconds(2 * (retry + 1)), cancellationToken);
            }
        }
    }

    /// <summary>
    /// 获取所有媒体文件
    /// </summary>
    public IEnumerable<MediaFile> GetAllMediaFiles()
    {
        return _mediaFiles.Values.ToList();
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}