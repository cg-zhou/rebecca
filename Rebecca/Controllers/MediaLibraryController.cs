using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Rebecca.Models;
using Rebecca.Services;
using System.IO;

namespace Rebecca.Controllers;

[ApiController]
[Route("api/medialibrary")]
public class MediaLibraryController : ControllerBase
{
    private readonly MediaLibraryService _mediaLibraryService;
    private readonly ILogger<MediaLibraryController> _logger;
    private readonly IContentTypeProvider _contentTypeProvider;

    public MediaLibraryController(MediaLibraryService mediaLibraryService, ILogger<MediaLibraryController> logger)
    {
        _mediaLibraryService = mediaLibraryService;
        _logger = logger;
        _contentTypeProvider = new FileExtensionContentTypeProvider();
    }

    // 获取媒体库配置
    [HttpGet("config")]
    public ActionResult<MediaLibraryConfig> GetConfig()
    {
        try
        {
            _logger.LogInformation("正在获取媒体库配置");
            var config = _mediaLibraryService.GetConfig();
            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取媒体库配置时发生错误");
            return StatusCode(500, new { message = "获取配置失败，请检查系统日志" });
        }
    }

    // 更新媒体库配置
    [HttpPut("config")]
    public ActionResult UpdateConfig([FromBody] MediaLibraryConfig config)
    {
        try
        {
            _logger.LogInformation("Updating media library configuration");
            if (config == null)
            {
                return BadRequest(new { message = "配置不能为空" });
            }

            _mediaLibraryService.SetConfig(config);
            return Ok(new { message = "配置已更新" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating media library configuration");
            return StatusCode(500, new { message = $"更新配置失败: {ex.Message}" });
        }
    }

    // 获取所有媒体文件信息
    [HttpGet("files")]
    public ActionResult GetMediaFiles()
    {
        try
        {
            _logger.LogInformation("Getting media files");
            var files = _mediaLibraryService.GetAllMediaFiles();
            return Ok(files);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting media files");
            return StatusCode(500, new { message = $"获取媒体文件失败: {ex.Message}" });
        }
    }

    // 开始扫描
    [HttpPost("scan")]
    public ActionResult StartScan()
    {
        try
        {
            _logger.LogInformation("正在启动媒体库扫描");
            if (_mediaLibraryService.IsScanning)
            {
                return BadRequest(new { message = "已有扫描任务在进行中，请等待完成" });
            }

            // 异步启动扫描，不等待完成
            _ = _mediaLibraryService.StartScanAsync();

            return Ok(new { message = "媒体库扫描已启动" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "启动扫描时发生错误");
            return StatusCode(500, new { message = "启动扫描失败，请检查系统日志" });
        }
    }

    // 取消扫描
    [HttpPost("scan/cancel")]
    public ActionResult CancelScan()
    {
        try
        {
            _logger.LogInformation("Cancelling media library scan");
            _mediaLibraryService.CancelScan();
            return Ok(new { message = "扫描已取消" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling media library scan");
            return StatusCode(500, new { message = $"取消扫描失败: {ex.Message}" });
        }
    }

    // 获取扫描状态
    [HttpGet("scan/status")]
    public ActionResult GetScanStatus()
    {
        try
        {
            var isScanning = _mediaLibraryService.IsScanning;
            _logger.LogDebug($"Scan status check: isScanning={isScanning}");
            return Ok(new { isScanning });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting scan status");
            return StatusCode(500, new { message = $"获取扫描状态失败: {ex.Message}" });
        }
    }

    // 图片代理接口
    [HttpGet("image/{*imagePath}")]
    public ActionResult GetImage([FromRoute] string imagePath)
    {
        try
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                return BadRequest(new { message = "图片路径不能为空" });
            }

            var fullPath = Path.GetFullPath(imagePath);
            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound(new { message = "图片文件不存在" });
            }

            // 获取文件的MIME类型
            if (!_contentTypeProvider.TryGetContentType(fullPath, out string? contentType))
            {
                contentType = "application/octet-stream";
            }

            // 读取文件并返回
            var fileStream = System.IO.File.OpenRead(fullPath);
            return File(fileStream, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取图片文件时发生错误: {Path}", imagePath);
            return StatusCode(500, new { message = "获取图片失败，请检查系统日志" });
        }
    }

    // 初始化并加载媒体文件
    [HttpPost("initialize")]
    public async Task<ActionResult> InitializeMediaLibrary()
    {
        try
        {
            _logger.LogInformation("正在初始化媒体库");
            await _mediaLibraryService.InitializeAndLoadFilesAsync();
            return Ok(new { message = "媒体库初始化完成" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "初始化媒体库时发生错误");
            return StatusCode(500, new { message = $"初始化媒体库失败: {ex.Message}" });
        }
    }

    // 处理单个文件
    [HttpPost("files/process")]
    public async Task<ActionResult> ProcessSingleFile([FromBody] ProcessFileRequest request)
    {
        try
        {
            _logger.LogInformation($"正在处理单个文件: {request.FilePath}");
            
            if (string.IsNullOrEmpty(request.FilePath))
            {
                return BadRequest(new { message = "文件路径不能为空" });
            }
            
            if (!System.IO.File.Exists(request.FilePath))
            {
                return NotFound(new { message = "指定的文件不存在" });
            }

            // 异步处理文件，不等待完成
            _ = _mediaLibraryService.ProcessSingleFileAsync(request.FilePath);
            
            return Ok(new { message = "文件处理已开始" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理单个文件时发生错误");
            return StatusCode(500, new { message = $"处理文件失败: {ex.Message}" });
        }
    }
}

// 处理单个文件的请求模型
public class ProcessFileRequest
{
    public string FilePath { get; set; } = string.Empty;
}