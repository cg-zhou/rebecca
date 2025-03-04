using Microsoft.Extensions.Logging;
using Rebecca.Models;
using Rebecca.Services.Interfaces;
using StdEx.Media.Tmdb.Models;
using System.IO;

namespace Rebecca.Services;

/// <summary>
/// 媒体库管理器，负责协调扫描流程
/// </summary>
public class MediaLibraryManager : IMediaLibraryManager
{
    private readonly ILogger<MediaLibraryManager> _logger;
    private readonly MediaLibraryConfigService _configService;
    private readonly IFileSystemService _fileSystemService;
    private readonly IMetadataService _metadataService;
    private readonly IMediaFileRepository _mediaFileRepository;
    private readonly INotificationService _notificationService;
    
    // 用于取消当前扫描任务
    private CancellationTokenSource? _scanCancellationTokenSource;
    
    /// <inheritdoc />
    public bool IsScanning { get; private set; } = false;

    public MediaLibraryManager(
        ILogger<MediaLibraryManager> logger,
        MediaLibraryConfigService configService,
        IFileSystemService fileSystemService,
        IMetadataService metadataService,
        IMediaFileRepository mediaFileRepository,
        INotificationService notificationService)
    {
        _logger = logger;
        _configService = configService;
        _fileSystemService = fileSystemService;
        _metadataService = metadataService;
        _mediaFileRepository = mediaFileRepository;
        _notificationService = notificationService;
    }

    /// <inheritdoc />
    public MediaLibraryConfig GetConfig()
    {
        return _configService.GetConfig();
    }

    /// <inheritdoc />
    public void SetConfig(MediaLibraryConfig config)
    {
        _configService.SetConfig(config);
    }

    /// <inheritdoc />
    public async Task StartScanAsync(CancellationToken cancellationToken = default)
    {
        if (IsScanning)
        {
            _logger.LogInformation("扫描已在进行中，忽略新的扫描请求");
            return;
        }

        try
        {
            IsScanning = true;
            await _notificationService.NotifyScanStatusAsync(true);
            
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
                    var files = _fileSystemService.CollectVideoFiles(path);
                    filesToProcess.AddRange(files);

                    // 快速添加到媒体文件列表，仅基本信息
                    foreach (var file in files)
                    {
                        if (!_mediaFileRepository.FileExists(file))
                        {
                            var mediaFile = _mediaFileRepository.CreateBasicMediaFileInfo(file);
                            _mediaFileRepository.AddOrUpdateFile(mediaFile);
                            await _notificationService.NotifyFileStatusAsync(mediaFile);
                        }
                    }
                }
                else
                {
                    _logger.LogWarning($"媒体库路径不存在: {path}");
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
            _logger.LogInformation("扫描操作已取消");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "扫描媒体库时出错");
            await _notificationService.NotifyErrorAsync("扫描过程发生错误");
        }
        finally
        {
            IsScanning = false;
            _scanCancellationTokenSource = null;
            await _notificationService.NotifyScanStatusAsync(false);
        }
    }

    /// <inheritdoc />
    public void CancelScan()
    {
        _scanCancellationTokenSource?.Cancel();
    }

    /// <inheritdoc />
    public IEnumerable<MediaFile> GetAllMediaFiles()
    {
        return _mediaFileRepository.GetAllFiles();
    }

    /// <inheritdoc />
    public Task InitializeAndLoadFilesAsync()
    {
        try
        {
            _logger.LogInformation("正在初始化并加载媒体文件信息");
            var config = GetConfig();
            var filesToProcess = new List<string>();

            // 扫描配置的所有媒体库路径
            foreach (var path in config.LibraryPaths)
            {
                if (Directory.Exists(path))
                {
                    var files = _fileSystemService.CollectVideoFiles(path);
                    filesToProcess.AddRange(files);

                    // 快速添加到媒体文件列表，仅基本信息
                    foreach (var file in files)
                    {
                        if (!_mediaFileRepository.FileExists(file))
                        {
                            // 文件首次添加
                            var newMediaFile = _mediaFileRepository.CreateBasicMediaFileInfo(file);
                            _mediaFileRepository.AddOrUpdateFile(newMediaFile);
                        }
                        else
                        {
                            // 文件已存在，检查元数据文件是否存在
                            var existingFile = _mediaFileRepository.GetFile(file);
                            if (existingFile != null)
                            {
                                _mediaFileRepository.CheckMetadataFiles(existingFile);
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogWarning($"媒体库路径不存在: {path}");
                }
            }
            
            _logger.LogInformation($"初始化完成，共加载 {_mediaFileRepository.GetAllFiles().Count()} 个媒体文件");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "初始化媒体库文件时出错");
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task ProcessSingleFileAsync(string filePath)
    {
        var mediaFile = _mediaFileRepository.GetFile(filePath);
        if (mediaFile == null)
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

    /// <summary>
    /// 处理单个视频文件
    /// </summary>
    private async Task ProcessVideoFileAsync(string filePath, CancellationToken cancellationToken)
    {
        // 首先检查文件是否存在，如果文件已被删除，则跳过处理
        if (!File.Exists(filePath))
        {
            _logger.LogWarning($"文件不存在，跳过处理: {filePath}");
            return;
        }

        // 获取文件的最后修改时间
        var fileInfo = new FileInfo(filePath);

        // 获取媒体文件信息，如果不存在则创建
        var mediaFile = _mediaFileRepository.GetFile(filePath);
        
        // 获取实际的元数据文件路径
        string posterPath = _fileSystemService.GetPosterPath(filePath);
        string fanartPath = _fileSystemService.GetFanartPath(filePath);
        string nfoPath = _fileSystemService.GetNfoPath(filePath);
        
        // 检查元数据文件是否实际存在（无论mediaFile对象中的状态如何）
        bool posterExists = File.Exists(posterPath);
        bool fanartExists = File.Exists(fanartPath);
        bool nfoExists = File.Exists(nfoPath);

        // 如果文件已经在字典中并且状态为已完成，则检查是否需要更新
        if (mediaFile?.Status == MediaFileStatus.Completed)
        {
            // 检查元数据文件是否被删除
            if (!posterExists || !fanartExists || !nfoExists)
            {
                _logger.LogInformation($"元数据文件不完整，需要重新处理: {filePath}");
                // 继续处理以重新生成元数据
            }
            // 如果文件的修改时间没变且所有元数据文件都存在，则跳过
            else if (mediaFile.LastScanned.HasValue && fileInfo.LastWriteTime <= mediaFile.LastScanned.Value)
            {
                return;
            }
        }

        // 创建或更新媒体文件信息
        mediaFile ??= _mediaFileRepository.CreateBasicMediaFileInfo(filePath);
        
        // 更新媒体文件中的元数据文件状态，确保与文件系统一致
        mediaFile.PosterPath = posterExists ? posterPath : null;
        mediaFile.FanartPath = fanartExists ? fanartPath : null;
        mediaFile.NfoPath = nfoExists ? nfoPath : null;

        try
        {
            mediaFile.Status = MediaFileStatus.Scanning;
            mediaFile.ProcessingComponent = ProcessingComponent.Scanning;
            _mediaFileRepository.AddOrUpdateFile(mediaFile);
            await _notificationService.NotifyFileStatusAsync(mediaFile);

            // 从文件名猜测电影名称
            string movieName = _fileSystemService.GetMovieName(Path.GetFileNameWithoutExtension(filePath));

            // 如果所有文件都已存在，则只更新媒体文件信息
            if (posterExists && fanartExists && nfoExists)
            {
                _logger.LogInformation($"所有元数据文件已存在: {filePath}");
                
                mediaFile.ProcessingComponent = ProcessingComponent.Nfo;
                _mediaFileRepository.AddOrUpdateFile(mediaFile);
                await _notificationService.NotifyFileStatusAsync(mediaFile);
                
                // 如果NFO存在，尝试从中读取电影信息
                try
                {
                    var nfo = await _metadataService.ReadNfoFileAsync(nfoPath);
                    if (nfo != null)
                    {
                        mediaFile.Status = MediaFileStatus.Completed;
                        mediaFile.ProcessingComponent = ProcessingComponent.None;
                        mediaFile.Title = nfo.Title;
                        mediaFile.Year = nfo.Year;
                        mediaFile.PosterPath = posterPath;
                        mediaFile.FanartPath = fanartPath;
                        mediaFile.NfoPath = nfoPath;
                        mediaFile.LastScanned = DateTime.Now;
                        _mediaFileRepository.AddOrUpdateFile(mediaFile);
                        await _notificationService.NotifyFileStatusAsync(mediaFile);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"读取现有NFO文件失败: {nfoPath}, 将重新生成元数据");
                    // 继续处理以重新生成元数据
                }
            }

            // 更新状态为下载元数据中
            mediaFile.Status = MediaFileStatus.Downloading;
            
            // 下载NFO信息
            mediaFile.ProcessingComponent = ProcessingComponent.Nfo;
            _mediaFileRepository.AddOrUpdateFile(mediaFile);
            await _notificationService.NotifyFileStatusAsync(mediaFile);
            
            // 从TMDB获取电影信息
            var movieNfo = await _metadataService.GetMovieMetadataAsync(movieName, cancellationToken);

            // 下载海报
            if (!posterExists && !string.IsNullOrEmpty(movieNfo.Art.Poster))
            {
                mediaFile.ProcessingComponent = ProcessingComponent.Poster;
                _mediaFileRepository.AddOrUpdateFile(mediaFile);
                await _notificationService.NotifyFileStatusAsync(mediaFile);
                
                _logger.LogInformation($"下载海报: {filePath}");
                await _metadataService.DownloadPosterAsync(movieNfo.Art.Poster, posterPath, cancellationToken);
                posterExists = true;  // 更新状态
                
                // 立即更新海报状态
                mediaFile.PosterPath = posterPath;
                _mediaFileRepository.AddOrUpdateFile(mediaFile);
                await _notificationService.NotifyFileStatusAsync(mediaFile);
            }

            // 下载背景图
            if (!fanartExists && !string.IsNullOrEmpty(movieNfo.Art.Fanart))
            {
                mediaFile.ProcessingComponent = ProcessingComponent.Fanart;
                _mediaFileRepository.AddOrUpdateFile(mediaFile);
                await _notificationService.NotifyFileStatusAsync(mediaFile);
                
                _logger.LogInformation($"下载背景图: {filePath}");
                await _metadataService.DownloadFanartAsync(movieNfo.Art.Fanart, fanartPath, cancellationToken);
                fanartExists = true;  // 更新状态
                
                // 立即更新背景图状态
                mediaFile.FanartPath = fanartPath;
                _mediaFileRepository.AddOrUpdateFile(mediaFile);
                await _notificationService.NotifyFileStatusAsync(mediaFile);
            }

            // 更新本地路径
            movieNfo.Art.LocalPoster = posterPath;
            movieNfo.Art.LocalFanart = fanartPath;

            // 如果NFO不存在，则生成
            if (!nfoExists)
            {
                mediaFile.ProcessingComponent = ProcessingComponent.Nfo;
                _mediaFileRepository.AddOrUpdateFile(mediaFile);
                await _notificationService.NotifyFileStatusAsync(mediaFile);
                
                _logger.LogInformation($"生成NFO文件: {filePath}");
                await _metadataService.CreateNfoFileAsync(movieNfo, nfoPath, cancellationToken);
                nfoExists = true;  // 更新状态
                
                // 立即更新NFO状态
                mediaFile.NfoPath = nfoPath;
                _mediaFileRepository.AddOrUpdateFile(mediaFile);
                await _notificationService.NotifyFileStatusAsync(mediaFile);
            }

            // 更新媒体文件信息
            mediaFile.Status = MediaFileStatus.Completed;
            mediaFile.ProcessingComponent = ProcessingComponent.None;
            mediaFile.Title = movieNfo.Title;
            mediaFile.Year = movieNfo.Year;
            mediaFile.PosterPath = posterExists ? posterPath : null;
            mediaFile.FanartPath = fanartExists ? fanartPath : null;
            mediaFile.NfoPath = nfoExists ? nfoPath : null;
            mediaFile.LastScanned = DateTime.Now;
            _mediaFileRepository.AddOrUpdateFile(mediaFile);
            await _notificationService.NotifyFileStatusAsync(mediaFile);

            _logger.LogInformation($"成功处理文件: {filePath}");
        }
        catch (Exception ex)
        {
            mediaFile.Status = MediaFileStatus.Error;
            mediaFile.ProcessingComponent = ProcessingComponent.None;
            mediaFile.ErrorMessage = ex.Message;
            _mediaFileRepository.AddOrUpdateFile(mediaFile);
            await _notificationService.NotifyFileStatusAsync(mediaFile);
            _logger.LogError(ex, $"处理文件时出错: {filePath}");
        }
    }
}