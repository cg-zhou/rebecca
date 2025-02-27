using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rebecca.Models;
using Rebecca.Services;
using System;
using System.Threading.Tasks;

namespace Rebecca.Controllers
{
    [ApiController]
    [Route("api/medialibrary")]
    public class MediaLibraryController : ControllerBase
    {
        private readonly MediaLibraryService _mediaLibraryService;
        private readonly ILogger<MediaLibraryController> _logger;
        
        public MediaLibraryController(MediaLibraryService mediaLibraryService, ILogger<MediaLibraryController> logger)
        {
            _mediaLibraryService = mediaLibraryService;
            _logger = logger;
        }
        
        // 获取媒体库配置
        [HttpGet("config")]
        public ActionResult<MediaLibraryConfig> GetConfig()
        {
            try
            {
                _logger.LogInformation("Getting media library configuration");
                var config = _mediaLibraryService.GetConfig();
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting media library configuration");
                return StatusCode(500, new { message = $"获取配置失败: {ex.Message}" });
            }
        }
        
        // 更新媒体库配置
        [HttpPost("config")]
        public ActionResult UpdateConfig([FromBody] MediaLibraryConfig config)
        {
            try
            {
                _logger.LogInformation("Updating media library configuration");
                if (config == null)
                {
                    return BadRequest(new { message = "配置不能为空" });
                }
                
                _mediaLibraryService.UpdateConfig(config);
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
        public async Task<ActionResult> StartScan()
        {
            try
            {
                _logger.LogInformation("Starting media library scan");
                if (_mediaLibraryService.IsScanning)
                {
                    return BadRequest(new { message = "扫描已在进行中" });
                }
                
                // 异步启动扫描，不等待完成
                _ = _mediaLibraryService.ScanLibrariesAsync();
                
                return Ok(new { message = "扫描已启动" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting media library scan");
                return StatusCode(500, new { message = $"启动扫描失败: {ex.Message}" });
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
    }
}