using Microsoft.Win32;
using System.Reflection;

namespace Rebecca.Services
{
    public class StartupService
    {
        private const string AppName = "Rebecca";
        private static readonly string AppPath = Assembly.GetExecutingAssembly().Location;
        private readonly RegistryKey? _registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

        public bool IsStartupEnabled()
        {
            if (_registryKey == null)
            {
                return false;
            }
            string? value = (string?)_registryKey.GetValue(AppName);
            return !string.IsNullOrEmpty(value) && value.Equals(AppPath, StringComparison.OrdinalIgnoreCase);
        }

        public void SetStartup(bool isEnabled)
        {
            if (_registryKey == null)
            {
                // Maybe log this? For now, we just can't do anything.
                return;
            }

            if (isEnabled)
            {
                _registryKey.SetValue(AppName, AppPath);
            }
            else
            {
                _registryKey.DeleteValue(AppName, false);
            }
        }
    }
}
