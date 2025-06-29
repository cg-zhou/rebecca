using System.Text.Json.Serialization;

namespace Rebecca.Models
{
    public class HotkeyConfig
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public HotkeyAction ActionId { get; set; }
        public string Key { get; set; } = string.Empty;
        public HotkeyModifiers Modifiers { get; set; }
    }

    public enum HotkeyModifiers : uint
    {
        None = 0,
        Alt = 0x0001,
        Control = 0x0002,
        Shift = 0x0004,
        Windows = 0x0008
    }
}