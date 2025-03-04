using StdEx.Media.Tmdb.Models;

namespace Rebecca.Services.Interfaces;

/// <summary>
/// 元数据服务接口，负责处理媒体元数据
/// </summary>
public interface IMetadataService
{
    /// <summary>
    /// 获取电影元数据
    /// </summary>
    /// <param name="movieName">电影名称</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>电影元数据</returns>
    Task<MovieNfo> GetMovieMetadataAsync(string movieName, CancellationToken cancellationToken);
    
    /// <summary>
    /// 下载电影海报
    /// </summary>
    /// <param name="url">海报URL</param>
    /// <param name="localPath">本地保存路径</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task DownloadPosterAsync(string url, string localPath, CancellationToken cancellationToken);
    
    /// <summary>
    /// 下载电影背景图
    /// </summary>
    /// <param name="url">背景图URL</param>
    /// <param name="localPath">本地保存路径</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task DownloadFanartAsync(string url, string localPath, CancellationToken cancellationToken);
    
    /// <summary>
    /// 创建NFO文件
    /// </summary>
    /// <param name="metadata">电影元数据</param>
    /// <param name="nfoPath">NFO文件保存路径</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task CreateNfoFileAsync(MovieNfo metadata, string nfoPath, CancellationToken cancellationToken);
    
    /// <summary>
    /// 读取现有的NFO文件
    /// </summary>
    /// <param name="nfoPath">NFO文件路径</param>
    /// <returns>电影元数据，如果读取失败返回null</returns>
    Task<MovieNfo?> ReadNfoFileAsync(string nfoPath);
}