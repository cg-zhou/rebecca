using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rebecca.Models;
using StdEx.Media.Tmdb;
using StdEx.Serialization;
using System.Collections.Concurrent;
using System.IO;

namespace Rebecca.Services
{
    public class MediaLibraryService : BackgroundService
    {
        private readonly ILogger<MediaLibraryService> _logger;
        private readonly ITmdbSettingsService _tmdbSettingsService;

        // 存储所有媒体文件的字典
        private readonly ConcurrentDictionary<string, MediaFile> _mediaFiles = new ConcurrentDictionary<string, MediaFile>();

        // 媒体库配置
        private MediaLibraryConfig _config = new MediaLibraryConfig();

        // 支持的视频文件扩展名
        private static readonly string[] VideoExtensions =
        [
            ".mp4", ".mkv", ".avi", ".mov",
            ".wmv", ".flv", ".m4v", ".rmvb"
        ];

        // 用于取消当前扫描任务
        private CancellationTokenSource? _scanCancellationTokenSource;

        public bool IsScanning { get; private set; } = false;

        public MediaLibraryService(ILogger<MediaLibraryService> logger, ITmdbSettingsService tmdbSettingsService)
        {
            _logger = logger;
            _tmdbSettingsService = tmdbSettingsService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Media Library Service is starting");

            try
            {
                LoadConfiguration();

                while (!stoppingToken.IsCancellationRequested)
                {
                    if (_config.AutoScan)
                    {
                        await ScanLibrariesAsync(stoppingToken);
                    }

                    // 等待配置的扫描间隔时间
                    await Task.Delay(TimeSpan.FromMinutes(_config.ScanIntervalMinutes), stoppingToken);
                }
            }
            catch (Exception ex) when (ex is not TaskCanceledException)
            {
                _logger.LogError(ex, "An error occurred in Media Library Service");
            }

            _logger.LogInformation("Media Library Service is stopping");
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        private void LoadConfiguration()
        {
            try
            {
                string configPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Rebecca", "media-library.json");

                if (File.Exists(configPath))
                {
                    string json = File.ReadAllText(configPath);
                    _config = JsonUtils.Deserialize<MediaLibraryConfig>(json) ?? new MediaLibraryConfig();
                }

                _logger.LogInformation($"Loaded media library configuration with {_config.LibraryPaths.Count} paths");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load media library configuration");
                _config = new MediaLibraryConfig();
            }
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        public void SaveConfiguration()
        {
            try
            {
                string appDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Rebecca");

                if (!Directory.Exists(appDataFolder))
                {
                    Directory.CreateDirectory(appDataFolder);
                }

                string configPath = Path.Combine(appDataFolder, "media-library.json");
                string json = JsonUtils.Serialize(_config);
                File.WriteAllText(configPath, json);

                _logger.LogInformation("Saved media library configuration");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save media library configuration");
            }
        }

        /// <summary>
        /// 获取配置
        /// </summary>
        public MediaLibraryConfig GetConfig()
        {
            return _config;
        }

        /// <summary>
        /// 更新配置
        /// </summary>
        public void UpdateConfig(MediaLibraryConfig config)
        {
            _config = config ?? new MediaLibraryConfig();
            SaveConfiguration();
        }

        /// <summary>
        /// 扫描所有媒体库
        /// </summary>
        public async Task ScanLibrariesAsync(CancellationToken cancellationToken = default)
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

                foreach (var path in _config.LibraryPaths)
                {
                    if (_scanCancellationTokenSource.Token.IsCancellationRequested)
                    {
                        break;
                    }

                    if (Directory.Exists(path))
                    {
                        await ScanFolderAsync(path, _scanCancellationTokenSource.Token);
                    }
                    else
                    {
                        _logger.LogWarning($"Library path does not exist: {path}");
                    }
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

            try
            {
                using var httpClient = new System.Net.Http.HttpClient();
                using var response = await httpClient.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync();
                using var fileStream = File.Create(localPath);
                await stream.CopyToAsync(fileStream, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to download image from {url} to {localPath}");
                throw;
            }
        }

        /// <summary>
        /// 获取所有媒体文件
        /// </summary>
        public IEnumerable<MediaFile> GetAllMediaFiles()
        {
            return _mediaFiles.Values.ToList();
        }
    }
}