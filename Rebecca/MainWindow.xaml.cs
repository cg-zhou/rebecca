using System.Windows;
using System.ComponentModel;
using Rebecca.Services;
using System.Diagnostics;

namespace Rebecca;

public partial class MainWindow : Window
{
    private HttpServer? _httpServer;
    private int _port;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
    }

    private async void MainWindow_Loaded(object? sender, RoutedEventArgs e)
    {
        try
        {
            await webView.EnsureCoreWebView2Async();
            _port = PortFinder.FindAvailable(8080);
            
            // 添加导航事件处理
            webView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
            
            _httpServer = new HttpServer(_port, GetType().Assembly);
            _httpServer.Start();

            webView.Source = new Uri($"http://localhost:{_port}/");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"启动失败: {ex.Message}");
            Close();
        }
    }

    private void CoreWebView2_NewWindowRequested(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NewWindowRequestedEventArgs e)
    {
        e.Handled = true;
        Process.Start(new ProcessStartInfo(e.Uri) { UseShellExecute = true });
    }

    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        _httpServer?.Dispose();
    }
}