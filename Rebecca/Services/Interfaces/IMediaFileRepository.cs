using Rebecca.Models;

namespace Rebecca.Services.Interfaces;

/// <summary>
/// 媒体文件仓库接口，负责管理媒体文件集合
/// </summary>
public interface IMediaFileRepository
{
    /// <summary>
    /// 添加或更新媒体文件
    /// </summary>
    /// <param name="file">媒体文件</param>
    void AddOrUpdateFile(MediaFile file);
    
    /// <summary>
    /// 根据路径获取媒体文件
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <returns>媒体文件，如果不存在则返回null</returns>
    MediaFile? GetFile(string path);
    
    /// <summary>
    /// 获取所有媒体文件
    /// </summary>
    /// <returns>媒体文件集合</returns>
    IEnumerable<MediaFile> GetAllFiles();
    
    /// <summary>
    /// 检查指定路径的媒体文件是否存在
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <returns>是否存在</returns>
    bool FileExists(string path);
    
    /// <summary>
    /// 创建基本的媒体文件信息
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>媒体文件</returns>
    MediaFile CreateBasicMediaFileInfo(string filePath);
    
    /// <summary>
    /// 检查并更新媒体文件的元数据文件状态
    /// </summary>
    /// <param name="mediaFile">媒体文件</param>
    void CheckMetadataFiles(MediaFile mediaFile);
}