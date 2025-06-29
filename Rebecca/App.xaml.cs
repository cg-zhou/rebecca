using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rebecca.Core.WebSockets;
using Rebecca.Services;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Application = System.Windows.Application;

namespace Rebecca;

public partial class App : Application
{
    private readonly ServiceCollection _services;
    private ServiceProvider? _serviceProvider;
    private WebHostService? _webHostService;

    public App()
    {
        // 添加全局异常处理
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        _services = new ServiceCollection();
        ConfigureServices();
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        LogService.Instance.Log($"UI Thread Exception: {e.Exception}");
        System.Windows.MessageBox.Show($"发生错误: {e.Exception.Message}\n\n日志位置: {Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Rebecca", "logs.txt")}",
                      "错误",
                      MessageBoxButton.OK,
                      MessageBoxImage.Error);
        e.Handled = true;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            LogService.Instance.Log($"Application Exception: {ex}");
        }
    }

    private void ConfigureServices()
    {
        // Add logging services
        _services.AddLogging(configure =>
        {
            configure.AddConsole();
            configure.AddDebug();
        });

        // Register HotkeyService once, with the Dispatcher
        _services.AddSingleton<HotkeyService>(provider => new HotkeyService(Current.Dispatcher));

        // Register WebHostService, injecting the already registered HotkeyService
        _services.AddSingleton<WebHostService>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<WebHostService>>();
            var hotkeyService = provider.GetRequiredService<HotkeyService>(); // Get the already registered instance
            return new WebHostService(logger, hotkeyService);
        });

        // Register other services
        _services.AddSingleton<StartupService>();
        _services.AddSingleton<VolumeService>();
        _services.AddSingleton<SettingsService>();
        _services.AddSingleton<Controllers.HotkeyController>();
        _services.AddSingleton<MainWindow>(provider =>
        {
            var webHostService = provider.GetRequiredService<WebHostService>();
            var startupService = provider.GetRequiredService<StartupService>();
            return new MainWindow(webHostService, startupService);
        });
        _services.AddSingleton<WebSocketHub>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _serviceProvider = _services.BuildServiceProvider();
        _webHostService = _serviceProvider.GetRequiredService<WebHostService>();
        var hotkeyService = _serviceProvider.GetRequiredService<HotkeyService>();
        var settingsService = _serviceProvider.GetRequiredService<SettingsService>();
        var volumeService = _serviceProvider.GetRequiredService<VolumeService>();

        // Load and register hotkeys
        var hotkeys = settingsService.LoadHotkeys();
        foreach (var hotkey in hotkeys)
        {
            Action action = hotkey.ActionId switch
            {
                "volume_up" => volumeService.VolumeUp,
                "volume_down" => volumeService.VolumeDown,
                _ => () => { }
            };
            hotkeyService.RegisterHotkey(hotkey.Key, (Services.HotkeyModifiers)hotkey.Modifiers, action);
        }

        // 启动Web服务
        await _webHostService.StartAsync();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_webHostService != null)
        {
            await _webHostService.StopAsync();
        }
        _serviceProvider?.GetRequiredService<HotkeyService>().Dispose();
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
