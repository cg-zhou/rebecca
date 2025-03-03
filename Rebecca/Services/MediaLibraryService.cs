using Microsoft.Extensions.Logging;
using Rebecca.Core.WebSockets;
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
    private readonly WebSocketHub _webSocketHub;

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
        MediaLibraryConfigService configService,
        WebSocketHub webSocketHub)
    {
        _logger = logger;
        _tmdbSettingsService = tmdbSettingsService;
        _configService = configService;
        _webSocketHub = webSocketHub;

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
            await _webSocketHub.BroadcastMessage(MessageType.ScanStatus, new { isScanning = true });
            
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
                            var mediaFile = new MediaFile
                            {
                                Path = file,
                                FileName = Path.GetFileName(file),
                                Status = MediaFileStatus.Pending,
                                Size = new FileInfo(file).Length
                            };
                            _mediaFiles[file] = mediaFile;
                            // 使用 await 确保消息被发送
                            await _webSocketHub.BroadcastMessage(MessageType.FileStatus, mediaFile);
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
            await _webSocketHub.BroadcastMessage(MessageType.Error, new { message = "扫描过程发生错误" });
        }
        finally
        {
            IsScanning = false;
            _scanCancellationTokenSource = null;
            await _webSocketHub.BroadcastMessage(MessageType.ScanStatus, new { isScanning = false });
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
            await _webSocketHub.BroadcastMessage(MessageType.FileStatus, mediaFile);

            // 从文件名猜测电影名称
            string movieName = GetMovieName(Path.GetFileNameWithoutExtension(filePath));

            // 设置本地文件路径
            string directory = Path.GetDirectoryName(filePath) ?? string.Empty;
            string baseName = Path.GetFileNameWithoutExtension(filePath);
            string posterPath = Path.Combine(directory, $"{baseName}-poster.jpg");
            string fanartPath = Path.Combine(directory, $"{baseName}-fanart.jpg");
            string nfoPath = Path.Combine(directory, $"{baseName}.nfo");

            // 检查哪些文件已存在
            bool posterExists = File.Exists(posterPath);
            bool fanartExists = File.Exists(fanartPath);
            bool nfoExists = File.Exists(nfoPath);

            // 如果所有文件都已存在，则只更新媒体文件信息
            if (posterExists && fanartExists && nfoExists)
            {
                _logger.LogInformation($"All metadata files already exist for: {filePath}");
                
                // 如果NFO存在，尝试从中读取电影信息
                try
                {
                    var nfo = XmlUtils.Deserialize<StdEx.Media.Tmdb.Models.MovieNfo>(File.ReadAllText(nfoPath));
                    mediaFile.Status = MediaFileStatus.Completed;
                    mediaFile.Title = nfo.Title;
                    mediaFile.Year = nfo.Year;
                    mediaFile.PosterPath = posterPath;
                    mediaFile.FanartPath = fanartPath;
                    mediaFile.NfoPath = nfoPath;
                    mediaFile.LastScanned = DateTime.Now;
                    _mediaFiles[filePath] = mediaFile;
                    await _webSocketHub.BroadcastMessage(MessageType.FileStatus, mediaFile);
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Failed to read existing NFO file: {nfoPath}, will re-generate metadata");
                    // 继续处理以重新生成元数据
                }
            }

            // 更新状态为下载元数据中
            mediaFile.Status = MediaFileStatus.Downloading;
            _mediaFiles[filePath] = mediaFile;
            await _webSocketHub.BroadcastMessage(MessageType.FileStatus, mediaFile);

            // 获取TMDB配置并创建TmdbUtils实例
            var config = await _tmdbSettingsService.GetConfigAsync();
            var tmdbUtils = new TmdbUtils(config);

            // 从TMDB获取电影信息
            var movieNfo = await tmdbUtils.GetMovieNfo(movieName);
            
            // 只下载缺少的文件
            if (!posterExists && !string.IsNullOrEmpty(movieNfo.Art.Poster))
            {
                _logger.LogInformation($"Downloading poster for: {filePath}");
                await DownloadImageAsync(movieNfo.Art.Poster, posterPath, cancellationToken);
            }

            if (!fanartExists && !string.IsNullOrEmpty(movieNfo.Art.Fanart))
            {
                _logger.LogInformation($"Downloading fanart for: {filePath}");
                await DownloadImageAsync(movieNfo.Art.Fanart, fanartPath, cancellationToken);
            }

            // 更新本地路径
            movieNfo.Art.LocalPoster = posterPath;
            movieNfo.Art.LocalFanart = fanartPath;

            // 如果NFO不存在，则生成
            if (!nfoExists)
            {
                _logger.LogInformation($"Generating NFO file for: {filePath}");
                string nfoXml = XmlUtils.Serialize(movieNfo);
                File.WriteAllText(nfoPath, nfoXml);
            }

            // 更新媒体文件信息
            mediaFile.Status = MediaFileStatus.Completed;
            mediaFile.Title = movieNfo.Title;
            mediaFile.Year = movieNfo.Year;
            mediaFile.PosterPath = posterPath;
            mediaFile.FanartPath = fanartPath;
            mediaFile.NfoPath = nfoPath;
            mediaFile.LastScanned = DateTime.Now;
            _mediaFiles[filePath] = mediaFile;
            await _webSocketHub.BroadcastMessage(MessageType.FileStatus, mediaFile);

            _logger.LogInformation($"Successfully processed file: {filePath}");
        }
        catch (Exception ex)
        {
            mediaFile.Status = MediaFileStatus.Error;
            mediaFile.ErrorMessage = ex.Message;
            _mediaFiles[filePath] = mediaFile;
            await _webSocketHub.BroadcastMessage(MessageType.FileStatus, mediaFile);
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

    /// <summary>
    /// 初始化时加载所有媒体文件基本信息，不下载元数据
    /// </summary>
    public async Task InitializeAndLoadFilesAsync()
    {
        try
        {
            _logger.LogInformation("初始化并加载媒体文件信息");
            var config = GetConfig();
            var filesToProcess = new List<string>();

            // 扫描配置的所有媒体库路径
            foreach (var path in config.LibraryPaths)
            {
                if (Directory.Exists(path))
                {
                    var files = CollectVideoFiles(path);
                    filesToProcess.AddRange(files);

                    // 快速添加到媒体文件列表，仅基本信息
                    foreach (var file in files)
                    {
                        if (!_mediaFiles.TryGetValue(file, out var existingFile))
                        {
                            // 文件首次添加
                            var newMediaFile = CreateBasicMediaFileInfo(file);
                            _mediaFiles[file] = newMediaFile;
                        }
                        else
                        {
                            // 文件已存在，检查元数据文件是否存在
                            CheckMetadataFiles(existingFile);
                        }
                    }
                }
                else
                {
                    _logger.LogWarning($"媒体库路径不存在: {path}");
                }
            }
            
            _logger.LogInformation($"初始化完成，共加载 {_mediaFiles.Count} 个媒体文件");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "初始化媒体库文件时出错");
        }
    }
    
    /// <summary>
    /// 为单个文件创建基本媒体信息对象
    /// </summary>
    private MediaFile CreateBasicMediaFileInfo(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        string directory = Path.GetDirectoryName(filePath) ?? string.Empty;
        string baseName = Path.GetFileNameWithoutExtension(filePath);
        
        // 设置可能的元数据文件路径
        string posterPath = Path.Combine(directory, $"{baseName}-poster.jpg");
        string fanartPath = Path.Combine(directory, $"{baseName}-fanart.jpg");
        string nfoPath = Path.Combine(directory, $"{baseName}.nfo");
        
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
                var movieNfo = XmlUtils.Deserialize<StdEx.Media.Tmdb.Models.MovieNfo>(File.ReadAllText(nfoPath));
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
    
    /// <summary>
    /// 检查媒体文件的元数据文件是否存在
    /// </summary>
    private void CheckMetadataFiles(MediaFile mediaFile)
    {
        if (mediaFile == null) return;
        
        string directory = Path.GetDirectoryName(mediaFile.Path) ?? string.Empty;
        string baseName = Path.GetFileNameWithoutExtension(mediaFile.Path);
        
        // 更新元数据文件路径和状态
        string posterPath = Path.Combine(directory, $"{baseName}-poster.jpg");
        string fanartPath = Path.Combine(directory, $"{baseName}-fanart.jpg");
        string nfoPath = Path.Combine(directory, $"{baseName}.nfo");
        
        mediaFile.PosterPath = File.Exists(posterPath) ? posterPath : null;
        mediaFile.FanartPath = File.Exists(fanartPath) ? fanartPath : null;
        mediaFile.NfoPath = File.Exists(nfoPath) ? nfoPath : null;
        
        // 如果所有元数据都存在但状态不是完成，则更新状态
        if (mediaFile.HasPoster && mediaFile.HasFanart && mediaFile.HasNfo && 
            mediaFile.Status != MediaFileStatus.Completed)
        {
            mediaFile.Status = MediaFileStatus.Completed;
        }
    }
    
    /// <summary>
    /// 处理单个视频文件的所有缺失元数据
    /// </summary>
    public async Task ProcessSingleFileAsync(string filePath)
    {
        if (!_mediaFiles.TryGetValue(filePath, out var mediaFile))
        {
            _logger.LogWarning($"找不到要处理的媒体文件: {filePath}");
            return;
        }
        
        // 如果已经在扫描中，则跳过
        if (mediaFile.Status == MediaFileStatus.Scanning || mediaFile.Status == MediaFileStatus.Downloading)
        {
            _logger.LogInformation($"文件已经在处理中: {filePath}");
            return;
        }
        
        // 使用取消令牌源以支持取消操作
        using var cts = new CancellationTokenSource();
        try
        {
            await ProcessVideoFileAsync(filePath, cts.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"处理单个文件时出错: {filePath}");
        }
    }
}