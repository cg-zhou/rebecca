using System.Runtime.InteropServices;

namespace Rebecca.Services
{
    public class HotkeyService : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        private static extern short VkKeyScanA(char ch);

        private const int WM_HOTKEY = 0x0312;
        private int _currentId;
        private readonly Dictionary<int, Action> _hotkeyActions;
        private readonly MessageWindow _messageWindow;
        private readonly System.Windows.Threading.Dispatcher _dispatcher;

        public HotkeyService(System.Windows.Threading.Dispatcher dispatcher)
        {
            _currentId = 0;
            _hotkeyActions = new Dictionary<int, Action>();
            _messageWindow = new MessageWindow();
            _messageWindow.HotkeyTriggered += OnHotkeyTriggered;
            _dispatcher = dispatcher;
        }

        public int RegisterHotkey(string key, HotkeyModifiers modifiers, Action action)
        {
            _currentId++;
            uint fsModifiers = (uint)modifiers;
            if (string.IsNullOrEmpty(key))
            {
                return 0;
            }
            short vk = VkKeyScanA(key.ToCharArray()[0]);

            if (vk == -1)
            {
                Console.WriteLine($"Invalid key: {key}");
                return -1;
            }

            int hotkeyId = _currentId;
            _dispatcher.Invoke(() =>
            {
                if (!RegisterHotKey(_messageWindow.Handle, hotkeyId, fsModifiers, (uint)vk))
                {
                    Console.WriteLine($"Failed to register hotkey: {modifiers} + {key}");
                }
            });

            _hotkeyActions[hotkeyId] = action;
            return hotkeyId;
        }

        public void UnregisterHotkey(int id)
        {
            _dispatcher.Invoke(() =>
            {
                UnregisterHotKey(_messageWindow.Handle, id);
            });
            _hotkeyActions.Remove(id);
        }

        private void OnHotkeyTriggered(int id)
        {
            if (_hotkeyActions.TryGetValue(id, out Action? action))
            {
                _dispatcher.Invoke(action);
            }
        }

        public void Dispose()
        {
            foreach (var id in _hotkeyActions.Keys)
            {
                UnregisterHotKey(_messageWindow.Handle, id);
            }
            _messageWindow.Dispose();
        }

        private class MessageWindow : Form
        {
            public event Action<int>? HotkeyTriggered;

            public MessageWindow()
            {
                // Create a hidden window to receive hotkey messages
                this.Text = "Hotkey Message Window";
                this.ShowInTaskbar = false;
                this.Opacity = 0;
                this.Load += (s, e) => this.Hide();
            }

            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);

                if (m.Msg == WM_HOTKEY)
                {
                    int id = m.WParam.ToInt32();
                    HotkeyTriggered?.Invoke(id);
                }
            }
        }
    }

    [Flags]
    public enum HotkeyModifiers : uint
    {
        None = 0,
        Alt = 0x0001,
        Control = 0x0002,
        Shift = 0x0004,
        Windows = 0x0008
    }
}
