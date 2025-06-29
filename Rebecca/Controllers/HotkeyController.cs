using Microsoft.AspNetCore.Mvc;
using Rebecca.Models;
using Rebecca.Services;

namespace Rebecca.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HotkeyController : ControllerBase
    {
        private readonly HotkeyService _hotkeyService;
        private readonly SettingsService _settingsService;
        private readonly VolumeService _volumeService;

        public HotkeyController(HotkeyService hotkeyService, SettingsService settingsService, VolumeService volumeService)
        {
            _hotkeyService = hotkeyService;
            _settingsService = settingsService;
            _volumeService = volumeService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<HotkeyConfig>> GetHotkeys()
        {
            return Ok(_settingsService.LoadHotkeys());
        }

        [HttpPost]
        public IActionResult AddHotkey([FromBody] HotkeyConfig hotkeyConfig)
        {
            var hotkeys = _settingsService.LoadHotkeys().ToList();
            hotkeyConfig.Id = hotkeys.Any() ? hotkeys.Max(h => h.Id) + 1 : 1;
            hotkeys.Add(hotkeyConfig);
            _settingsService.SaveHotkeys(hotkeys);
            RegisterHotkey(hotkeyConfig);
            return Ok(hotkeyConfig);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateHotkey(int id, [FromBody] HotkeyConfig hotkeyConfig)
        {
            var hotkeys = _settingsService.LoadHotkeys().ToList();
            var existingHotkey = hotkeys.FirstOrDefault(h => h.Id == id);
            if (existingHotkey == null)
            {
                return NotFound();
            }

            UnregisterHotkey(existingHotkey);

            existingHotkey.Key = hotkeyConfig.Key;
            existingHotkey.Modifiers = hotkeyConfig.Modifiers;
            existingHotkey.ActionId = hotkeyConfig.ActionId;

            _settingsService.SaveHotkeys(hotkeys);
            RegisterHotkey(existingHotkey);
            return Ok(existingHotkey);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteHotkey(int id)
        {
            var hotkeys = _settingsService.LoadHotkeys().ToList();
            var hotkeyToDelete = hotkeys.FirstOrDefault(h => h.Id == id);
            if (hotkeyToDelete == null)
            {
                return NotFound();
            }

            UnregisterHotkey(hotkeyToDelete);
            hotkeys.Remove(hotkeyToDelete);
            _settingsService.SaveHotkeys(hotkeys);
            return NoContent();
        }

        private void RegisterHotkey(HotkeyConfig hotkeyConfig)
        {
            Action action = hotkeyConfig.ActionId switch
            {
                "volume_up" => _volumeService.VolumeUp,
                "volume_down" => _volumeService.VolumeDown,
                _ => () => { }
            };
            _hotkeyService.RegisterHotkey(hotkeyConfig.Key, (Services.HotkeyModifiers)hotkeyConfig.Modifiers, action);
        }

        private void UnregisterHotkey(HotkeyConfig hotkeyConfig)
        {
            _hotkeyService.UnregisterHotkey(hotkeyConfig.Id);
        }
    }
}
