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
            var list = _settingsService.LoadHotkeys().ToList();

            // Ensure default hotkeys exist
            if (!list.Any(x => x.ActionId == HotkeyAction.VolumeUp))
            {
                list.Add(new HotkeyConfig
                {
                    ActionId = HotkeyAction.VolumeUp,
                    Key = string.Empty,
                    Modifiers = HotkeyModifiers.None
                });
            }
            if (!list.Any(x => x.ActionId == HotkeyAction.VolumeDown))
            {
                list.Add(new HotkeyConfig
                {
                    ActionId = HotkeyAction.VolumeDown,
                    Key = string.Empty,
                    Modifiers = HotkeyModifiers.None
                });
            }
            return Ok(list);
        }

        [HttpPost("set")]
        public IActionResult SetHotkey([FromBody] HotkeyConfig hotkeyConfig)
        {
            var hotkeys = _settingsService.LoadHotkeys().ToList();
            var existingHotkey = hotkeys.FirstOrDefault(h => h.ActionId == hotkeyConfig.ActionId);

            if (existingHotkey != null)
            {
                // Update existing hotkey
                UnregisterHotkey(existingHotkey.ActionId); // Unregister by ActionId
                existingHotkey.Key = hotkeyConfig.Key;
                existingHotkey.Modifiers = hotkeyConfig.Modifiers;
            }
            else
            {
                // Add new hotkey (ActionId is already set in hotkeyConfig)
                hotkeys.Add(hotkeyConfig);
                existingHotkey = hotkeyConfig; // Reference the newly added hotkey
            }

            _settingsService.SaveHotkeys(hotkeys);
            RegisterHotkey(existingHotkey.ActionId, existingHotkey.Key, existingHotkey.Modifiers);

            // Return a fresh copy to ensure all properties are correctly serialized
            return Ok(new HotkeyConfig
            {
                ActionId = existingHotkey.ActionId,
                Key = existingHotkey.Key,
                Modifiers = existingHotkey.Modifiers
            });
        }

        [HttpDelete("clear/{actionId}")]
        public IActionResult ClearHotkey(HotkeyAction actionId)
        {
            var hotkeys = _settingsService.LoadHotkeys().ToList();
            var hotkeyToClear = hotkeys.FirstOrDefault(h => h.ActionId == actionId);

            if (hotkeyToClear == null)
            {
                return NotFound();
            }

            UnregisterHotkey(hotkeyToClear.ActionId); // Unregister by ActionId
            hotkeys.Remove(hotkeyToClear);
            _settingsService.SaveHotkeys(hotkeys);
            return NoContent();
        }

        private void RegisterHotkey(HotkeyAction actionId, string key, HotkeyModifiers modifiers)
        {
            Action action = actionId switch
            {
                HotkeyAction.VolumeUp => _volumeService.VolumeUp,
                HotkeyAction.VolumeDown => _volumeService.VolumeDown,
                _ => () => { }
            };
            _hotkeyService.RegisterHotkey(actionId, key, modifiers, action);
        }

        private void UnregisterHotkey(HotkeyAction actionId)
        {
            _hotkeyService.UnregisterHotkey(actionId);
        }
    }
}