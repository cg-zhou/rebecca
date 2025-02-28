using Microsoft.Extensions.Logging;
using Rebecca.Models;
using StdEx.Media.Tmdb;
using StdEx.Media.Tmdb.Models;
using StdEx.Serialization;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Rebecca.Services;

public interface ITmdbSettingsService
{
    Task<TmdbConfig> GetConfigAsync();
    Task SaveConfigAsync(TmdbConfigRequest request);
    Task<(bool success, string message)> TestConnectionAsync(TmdbConfigRequest request);
}

public class TmdbSettingsService : ITmdbSettingsService
{
    private readonly string _tmdbConfigPath;
    private readonly ILogger<TmdbSettingsService> _logger;

    public TmdbSettingsService(ILogger<TmdbSettingsService> logger)
    {
        _logger = logger;
        
        // 设置TMDB配置文件路径
        _tmdbConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Rebecca", "tmdb-settings.json");
            
        // 确保配置目录存在
        var directory = Path.GetDirectoryName(_tmdbConfigPath);
        if (!Directory.Exists(directory) && directory != null)
        {
            Directory.CreateDirectory(directory);
        }
    }

    public async Task<TmdbConfig> GetConfigAsync()
    {
        try
        {
            _logger.LogInformation("Getting TMDB configuration");
            if (File.Exists(_tmdbConfigPath))
            {
                var json = await File.ReadAllTextAsync(_tmdbConfigPath);
                var config = JsonUtils.Deserialize<TmdbConfig>(json);
                if (config != null)
                {
                    return config;
                }
            }
            
            // 如果找不到配置或者配置无效，返回默认配置
            return new TmdbConfig();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading TMDB configuration");
            throw;
        }
    }

    public async Task SaveConfigAsync(TmdbConfigRequest request)
    {
        try
        {
            _logger.LogInformation("Saving TMDB configuration");
            if (string.IsNullOrEmpty(request.BearerToken))
            {
                throw new ArgumentException("TMDB API Token 不能为空");
            }
            
            // 创建TmdbConfig
            var config = new TmdbConfig
            {
                BearerToken = request.BearerToken,
                BaseApiUrl = request.BaseApiUrl ?? TmdbConfig.DefaultBaseApiUrl,
                BaseImageUrl = request.BaseImageUrl ?? TmdbConfig.DefaultBaseImageUrl,
                Language = request.Language ?? TmdbConfig.DefaultLanguage,
                ApiKeyType = request.ApiKeyType ?? "v4"
            };
            
            // 序列化并保存
            var json = JsonUtils.Serialize(config);
            await File.WriteAllTextAsync(_tmdbConfigPath, json);
            
            _logger.LogInformation("TMDB configuration saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving TMDB configuration");
            throw;
        }
    }

    public async Task<(bool success, string message)> TestConnectionAsync(TmdbConfigRequest request)
    {
        try
        {
            _logger.LogInformation("Testing TMDB API connection");
            if (string.IsNullOrEmpty(request.BearerToken))
            {
                return (false, "TMDB API Token 不能为空");
            }
            
            // 创建临时TmdbConfig用于测试
            var config = new TmdbConfig
            {
                BearerToken = request.BearerToken,
                BaseApiUrl = request.BaseApiUrl ?? TmdbConfig.DefaultBaseApiUrl,
                BaseImageUrl = request.BaseImageUrl ?? TmdbConfig.DefaultBaseImageUrl,
                Language = request.Language ?? TmdbConfig.DefaultLanguage,
                ApiKeyType = request.ApiKeyType ?? "v4"
            };
            
            // 创建临时TmdbUtils实例进行测试
            var tmdbUtils = new TmdbUtils(config, 10);
            
            // 尝试获取一个电影作为测试
            try
            {
                _logger.LogInformation($"Testing TMDB API with config: ApiKeyType={config.ApiKeyType}, BaseApiUrl={config.BaseApiUrl}");
                var movie = await tmdbUtils.GetMovieNfo("Inception");
                _logger.LogInformation($"TMDB API test successful, found movie: {movie.Title}");
                return (true, $"成功连接TMDB API并获取电影: {movie.Title}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TMDB API call failed");
                return (false, $"TMDB API 调用错误: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing TMDB API");
            throw;
        }
    }
}