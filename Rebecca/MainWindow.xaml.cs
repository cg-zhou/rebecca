using System.Windows;
using System.ComponentModel;
using Rebecca.Services;

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

    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        _httpServer?.Dispose();
    }
}