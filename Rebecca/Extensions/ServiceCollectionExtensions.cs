using Microsoft.Extensions.DependencyInjection;
using Rebecca.Core.WebSockets;
using Rebecca.Services;
using Rebecca.Services.Interfaces;
using System;

namespace Rebecca.Extensions;

/// <summary>
/// 用于扩展服务注册的扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注册媒体库相关的所有服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合，用于方法链</returns>
    public static IServiceCollection AddMediaLibraryServices(this IServiceCollection services)
    {
        // 注册各个服务的实现
        services.AddSingleton<IFileSystemService, FileSystemService>();
        services.AddSingleton<IDownloadService, DownloadService>();
        services.AddSingleton<IMetadataService, MetadataService>();
        services.AddSingleton<IMediaFileRepository, MediaFileRepository>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<MediaLibraryConfigService>();
        services.AddSingleton<ITmdbSettingsService, TmdbSettingsService>();
        
        // 注册媒体库管理器
        services.AddSingleton<IMediaLibraryManager, MediaLibraryManager>();
        
        // 为了向后兼容，注册适配器作为MediaLibraryService
        services.AddSingleton<MediaLibraryService, MediaLibraryServiceAdapter>();
        
        return services;
    }
}