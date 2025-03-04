namespace Rebecca.Services.Interfaces;

/// <summary>
/// 文件系统服务接口，负责文件系统相关操作
/// </summary>
public interface IFileSystemService
{
    /// <summary>
    /// 收集指定文件夹中的所有视频文件
    /// </summary>
    /// <param name="folderPath">要扫描的文件夹路径</param>
    /// <returns>视频文件路径集合</returns>
    IEnumerable<string> CollectVideoFiles(string folderPath);

    /// <summary>
    /// 检查文件是否为视频文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>是否为视频文件</returns>
    bool IsVideoFile(string filePath);

    /// <summary>
    /// 从文件名猜测电影名称
    /// </summary>
    /// <param name="fileName">文件名（不包含扩展名）</param>
    /// <returns>猜测的电影名称</returns>
    string GetMovieName(string fileName);

    /// <summary>
    /// 获取与视频文件关联的海报图片路径
    /// </summary>
    /// <param name="videoPath">视频文件路径</param>
    /// <returns>海报图片路径</returns>
    string GetPosterPath(string videoPath);

    /// <summary>
    /// 获取与视频文件关联的背景图片路径
    /// </summary>
    /// <param name="videoPath">视频文件路径</param>
    /// <returns>背景图片路径</returns>
    string GetFanartPath(string videoPath);

    /// <summary>
    /// 获取与视频文件关联的NFO文件路径
    /// </summary>
    /// <param name="videoPath">视频文件路径</param>
    /// <returns>NFO文件路径</returns>
    string GetNfoPath(string videoPath);
}