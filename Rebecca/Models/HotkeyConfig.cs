namespace Rebecca.Models
{
    public class HotkeyConfig
    {
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public HotkeyModifiers Modifiers { get; set; }
        public string ActionId { get; set; } = string.Empty;
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
