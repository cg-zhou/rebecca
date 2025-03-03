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
    private MediaLibraryService? _mediaLibraryService;

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
        
        _services.AddSingleton<WebHostService>();
        _services.AddSingleton<MainWindow>();
        _services.AddSingleton<WebSocketHub>();
        _services.AddSingleton<MediaLibraryConfigService>();
        _services.AddSingleton<ITmdbSettingsService, TmdbSettingsService>();
        _services.AddSingleton<MediaLibraryService>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _serviceProvider = _services.BuildServiceProvider();
        _webHostService = _serviceProvider.GetRequiredService<WebHostService>();
        _mediaLibraryService = _serviceProvider.GetRequiredService<MediaLibraryService>();
        
        // 启动Web服务
        await _webHostService.StartAsync();

        // 初始化媒体库（加载所有文件信息，但不下载元数据）
        await _mediaLibraryService.InitializeAndLoadFilesAsync();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_webHostService != null)
        {
            await _webHostService.StopAsync();
        }
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
