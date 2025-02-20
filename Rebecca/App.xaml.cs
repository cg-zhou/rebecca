using Microsoft.Extensions.DependencyInjection;
using Rebecca.Services;
using System.Windows;
using Application = System.Windows.Application;

namespace Rebecca
{
    public partial class App : Application
    {
        private readonly ServiceCollection _services;
        private ServiceProvider? _serviceProvider;
        private WebHostService? _webHostService;

        public App()
        {
            _services = new ServiceCollection();
            ConfigureServices();
        }

        private void ConfigureServices()
        {
            _services.AddSingleton<PortFinder>();
            _services.AddSingleton<WebHostService>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _serviceProvider = _services.BuildServiceProvider();
            _webHostService = _serviceProvider.GetRequiredService<WebHostService>();
            await _webHostService.StartAsync();
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
}
