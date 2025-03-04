using System.IO;

namespace Rebecca.Services.Interfaces;

/// <summary>
/// 下载服务接口，负责网络文件下载
/// </summary>
public interface IDownloadService
{
    /// <summary>
    /// 下载文件到流
    /// </summary>
    /// <param name="url">URL地址</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>文件流</returns>
    Task<Stream> DownloadFileAsync(string url, CancellationToken cancellationToken);
    
    /// <summary>
    /// 下载文件到本地路径
    /// </summary>
    /// <param name="url">URL地址</param>
    /// <param name="localPath">本地保存路径</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task DownloadToFileAsync(string url, string localPath, CancellationToken cancellationToken);
}