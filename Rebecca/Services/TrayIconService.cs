
using System.Windows;
using Application = System.Windows.Application;

namespace Rebecca.Services;

public class TrayIconService : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly Window _mainWindow;
    private readonly StartupService _startupService;

    public TrayIconService(Window mainWindow, StartupService startupService)
    {
        _mainWindow = mainWindow;
        _startupService = startupService;
        _notifyIcon = new NotifyIcon
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ResourceAssembly.Location),
            Visible = true,
            Text = "Rebecca"
        };

        InitializeContextMenu();
        InitializeEvents();
    }

    private void InitializeContextMenu()
    {
        var contextMenu = new ContextMenuStrip();
        var openItem = new ToolStripMenuItem("打开");
        var settingsItem = new ToolStripMenuItem("设置");
        var startupItem = new ToolStripMenuItem("开机启动");
        var exitItem = new ToolStripMenuItem("退出");

        startupItem.CheckOnClick = true;
        startupItem.Checked = _startupService.IsStartupEnabled();
        startupItem.Click += (s, e) => _startupService.SetStartup(startupItem.Checked);

        settingsItem.DropDownItems.Add(startupItem);

        openItem.Click += (s, e) => ShowMainWindow();
        exitItem.Click += (s, e) => ExitApplication();

        contextMenu.Items.Add(openItem);
        contextMenu.Items.Add(settingsItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(exitItem);
        _notifyIcon.ContextMenuStrip = contextMenu;
    }

    private void InitializeEvents()
    {
        _notifyIcon.MouseDoubleClick += (s, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                ShowMainWindow();
            }
        };
    }

    public void ShowMainWindow()
    {
        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    public void MinimizeToTray()
    {
        _mainWindow.Hide();
    }

    private void ExitApplication()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        Application.Current.Shutdown();
    }

    public void Dispose()
    {
        _notifyIcon.Dispose();
    }
}

