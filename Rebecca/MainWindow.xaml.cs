﻿using Microsoft.Web.WebView2.Core;
using Rebecca.Services;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace Rebecca;

public partial class MainWindow : Window
{
    private readonly TrayIconService _trayIconService;

    public MainWindow()
    {
        InitializeComponent();
        _trayIconService = new TrayIconService(this);
        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
        StateChanged += MainWindow_StateChanged;
    }

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            _trayIconService.MinimizeToTray();
        }
    }

    private async void MainWindow_Loaded(object? sender, RoutedEventArgs e)
    {
        try
        {
            await webView.EnsureCoreWebView2Async();
            webView.CoreWebView2.Settings.IsStatusBarEnabled = false;
            webView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
            webView.Source = new Uri($"http://localhost:{4074}/");
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"启动失败: {ex.Message}");
            Close();
        }
    }

    private void CoreWebView2_NewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs e)
    {
        e.Handled = true;
        var processStartInfo = new ProcessStartInfo(e.Uri) { UseShellExecute = true };
        Process.Start(processStartInfo);
    }

    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        if (e.Cancel == false)
        {
            _trayIconService.Dispose();
        }
    }
}