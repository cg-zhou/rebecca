using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rebecca.Models;
using Rebecca.Services;
using StdEx.Media.Tmdb.Models;
using System;
using System.Threading.Tasks;

namespace Rebecca.Controllers
{
    [ApiController]
    [Route("api/settings")]
    public class SettingsController : ControllerBase
    {
        private readonly ILogger<SettingsController> _logger;
        private readonly ITmdbSettingsService _tmdbSettingsService;
        
        public SettingsController(ILogger<SettingsController> logger, ITmdbSettingsService tmdbSettingsService)
        {
            _logger = logger;
            _tmdbSettingsService = tmdbSettingsService;
        }
        
        // 获取TMDB配置
        [HttpGet("tmdb")]
        public async Task<ActionResult<object>> GetTmdbConfig()
        {
            try
            {
                var config = await _tmdbSettingsService.GetConfigAsync();
                
                // 不返回实际token，只返回是否已配置
                var safeConfig = new
                {
                    bearerToken = !string.IsNullOrEmpty(config.BearerToken) ? "********" : "",
                    baseApiUrl = config.BaseApiUrl,
                    baseImageUrl = config.BaseImageUrl,
                    language = config.Language,
                    apiKeyType = config.ApiKeyType ?? "v4"
                };
                
                return Ok(safeConfig);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading TMDB configuration");
                return StatusCode(500, new { error = $"加载TMDB配置失败: {ex.Message}" });
            }
        }
        
        // 保存TMDB配置
        [HttpPost("tmdb")]
        public async Task<ActionResult> SaveTmdbConfig([FromBody] TmdbConfigRequest request)
        {
            try
            {
                await _tmdbSettingsService.SaveConfigAsync(request);
                return Ok(new { message = "TMDB配置保存成功" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving TMDB configuration");
                return StatusCode(500, new { error = $"保存TMDB配置失败: {ex.Message}" });
            }
        }
        
        // 测试TMDB API连接
        [HttpPost("tmdb/test")]
        public async Task<ActionResult> TestTmdbApi([FromBody] TmdbConfigRequest request)
        {
            try
            {
                var (success, message) = await _tmdbSettingsService.TestConnectionAsync(request);
                return Ok(new { success, message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing TMDB API");
                return StatusCode(500, new { success = false, message = $"测试TMDB API失败: {ex.Message}" });
            }
        }
    }
}