
using Microsoft.AspNetCore.Mvc;
using Rebecca.Services;

namespace Rebecca.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly StartupService _startupService;

        public SettingsController(StartupService startupService)
        {
            _startupService = startupService;
        }

        [HttpGet("startup")]
        public IActionResult GetStartup()
        { 
            return Ok(new { enabled = _startupService.IsStartupEnabled() });
        }

        [HttpPost("startup")]
        public IActionResult SetStartup([FromBody] StartupRequest request)
        { 
            _startupService.SetStartup(request.Enabled);
            return Ok(new { success = true });
        }
    }

    public class StartupRequest
    {
        public bool Enabled { get; set; }
    }
}
