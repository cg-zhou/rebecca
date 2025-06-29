using System.Runtime.InteropServices;
using Rebecca.Models;

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
        private int _nextSystemHotkeyId; // Used to generate unique system-wide hotkey IDs
        private readonly Dictionary<int, Action> _systemHotkeyIdToActions; // Maps system-assigned ID to Action
        private readonly Dictionary<HotkeyAction, int> _actionToSystemHotkeyId; // Maps HotkeyAction to system-assigned ID
        private readonly MessageWindow _messageWindow;
        private readonly System.Windows.Threading.Dispatcher _dispatcher;

        public HotkeyService(System.Windows.Threading.Dispatcher dispatcher)
        {
            _nextSystemHotkeyId = 1; // Start from 1
            _systemHotkeyIdToActions = new Dictionary<int, Action>();
            _actionToSystemHotkeyId = new Dictionary<HotkeyAction, int>();
            _messageWindow = new MessageWindow();
            _messageWindow.HotkeyTriggered += OnHotkeyTriggered;
            _dispatcher = dispatcher;
        }

        public void RegisterHotkey(HotkeyAction actionId, string key, HotkeyModifiers modifiers, Action action)
        {
            // If this actionId already has a hotkey registered, unregister it first
            if (_actionToSystemHotkeyId.TryGetValue(actionId, out int existingSystemId))
            {
                UnregisterHotKeyInternal(existingSystemId);
                _actionToSystemHotkeyId.Remove(actionId);
                _systemHotkeyIdToActions.Remove(existingSystemId);
            }

            uint fsModifiers = (uint)modifiers;
            if (string.IsNullOrEmpty(key))
            {
                Console.WriteLine($"Cannot register hotkey for {actionId}: Key is empty.");
                return; // Do not register if key is empty
            }

            short vk = VkKeyScanA(key.ToCharArray()[0]);

            if (vk == -1)
            {
                Console.WriteLine($"Invalid key for {actionId}: {key}");
                return; // Do not register if key is invalid
            }

            int newSystemId = _nextSystemHotkeyId++;

            _dispatcher.Invoke(() =>
            {
                if (!RegisterHotKey(_messageWindow.Handle, newSystemId, fsModifiers, (uint)vk))
                {
                    Console.WriteLine($"Failed to register hotkey for {actionId}: {modifiers} + {key}");
                }
            });

            _actionToSystemHotkeyId[actionId] = newSystemId;
            _systemHotkeyIdToActions[newSystemId] = action;
        }

        public void UnregisterHotkey(HotkeyAction actionId)
        {
            if (_actionToSystemHotkeyId.TryGetValue(actionId, out int systemId))
            {
                UnregisterHotKeyInternal(systemId);
                _actionToSystemHotkeyId.Remove(actionId);
                _systemHotkeyIdToActions.Remove(systemId);
            }
        }

        private void UnregisterHotKeyInternal(int systemId)
        {
            _dispatcher.Invoke(() =>
            {
                UnregisterHotKey(_messageWindow.Handle, systemId);
            });
        }

        private void OnHotkeyTriggered(int id)
        {
            if (_systemHotkeyIdToActions.TryGetValue(id, out Action? action))
            {
                _dispatcher.Invoke(action);
            }
        }

        public void Dispose()
        {
            foreach (var systemId in _systemHotkeyIdToActions.Keys.ToList()) // ToList to avoid modifying collection during iteration
            {
                UnregisterHotKeyInternal(systemId);
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
}