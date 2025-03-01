using Microsoft.Extensions.Logging;
using Rebecca.Models;
using StdEx.Serialization;
using System.IO;

namespace Rebecca.Services;

public class MediaLibraryConfigService
{
    private readonly ILogger<MediaLibraryConfigService> _logger;

    public MediaLibraryConfigService(ILogger<MediaLibraryConfigService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 获取配置
    /// </summary>
    public MediaLibraryConfig GetConfig()
    {
        var configPath = GetConfigPath();

        if (!File.Exists(configPath))
        {
            _logger.LogInformation($"Config file not found, creating default at {configPath}");
            var defaultConfig = new MediaLibraryConfig
            {
                LibraryPaths = Array.Empty<string>()
            };
            SetConfig(defaultConfig);
            return defaultConfig;
        }

        var json = File.ReadAllText(configPath);
        return JsonUtils.Deserialize<MediaLibraryConfig>(json);
    }

    /// <summary>
    /// 更新配置
    /// </summary>
    public void SetConfig(MediaLibraryConfig config)
    {
        var json = JsonUtils.Serialize(config);
        var configPath = GetConfigPath();
        File.WriteAllText(configPath, json);
    }

    /// <summary>
    /// 获取配置文件路径
    /// </summary>
    public string GetConfigPath()
    {
        var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var rebeccaConfigFolder = Path.Combine(appDataFolder, "Rebecca");

        if (!Directory.Exists(rebeccaConfigFolder))
        {
            Directory.CreateDirectory(rebeccaConfigFolder);
        }

        return Path.Combine(rebeccaConfigFolder, "media-library.json");
    }
}