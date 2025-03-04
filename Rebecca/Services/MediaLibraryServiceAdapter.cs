using Microsoft.Extensions.Logging;
using Rebecca.Core.WebSockets;
using Rebecca.Models;
using Rebecca.Services.Interfaces;

namespace Rebecca.Services;

/// <summary>
/// MediaLibraryService适配器，提供与旧版MediaLibraryService相同的功能，
/// 但将所有调用转发给新的IMediaLibraryManager实现。
/// 这个类是为了向后兼容而设计的，新代码应该直接使用IMediaLibraryManager。
/// </summary>
public class MediaLibraryServiceAdapter : MediaLibraryService
{
    private readonly IMediaLibraryManager _mediaLibraryManager;
    private readonly ILogger<MediaLibraryServiceAdapter> _logger;

    public MediaLibraryServiceAdapter(
        IMediaLibraryManager mediaLibraryManager,
        ILogger<MediaLibraryServiceAdapter> logger,
        ITmdbSettingsService tmdbSettingsService,
        MediaLibraryConfigService configService,
        WebSocketHub webSocketHub)
        : base(logger, tmdbSettingsService, configService, webSocketHub)
    {
        _mediaLibraryManager = mediaLibraryManager;
        _logger = logger;
    }

    public new bool IsScanning => _mediaLibraryManager.IsScanning;

    public new MediaLibraryConfig GetConfig()
    {
        _logger.LogDebug("通过适配器调用GetConfig");
        return _mediaLibraryManager.GetConfig();
    }

    public new void SetConfig(MediaLibraryConfig config)
    {
        _logger.LogDebug("通过适配器调用SetConfig");
        _mediaLibraryManager.SetConfig(config);
    }

    public new async Task StartScanAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("通过适配器调用StartScanAsync");
        await _mediaLibraryManager.StartScanAsync(cancellationToken);
    }

    public new void CancelScan()
    {
        _logger.LogDebug("通过适配器调用CancelScan");
        _mediaLibraryManager.CancelScan();
    }

    public new IEnumerable<MediaFile> GetAllMediaFiles()
    {
        _logger.LogDebug("通过适配器调用GetAllMediaFiles");
        return _mediaLibraryManager.GetAllMediaFiles();
    }

    public new async Task InitializeAndLoadFilesAsync()
    {
        _logger.LogDebug("通过适配器调用InitializeAndLoadFilesAsync");
        await _mediaLibraryManager.InitializeAndLoadFilesAsync();
    }

    public new async Task ProcessSingleFileAsync(string filePath)
    {
        _logger.LogDebug("通过适配器调用ProcessSingleFileAsync");
        await _mediaLibraryManager.ProcessSingleFileAsync(filePath);
    }
}