using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Rebecca.Services
{
    public class WebHostService
    {
        private WebApplication? _app;
        private readonly PortFinder _portFinder;
        public int Port { get; private set; }

        public WebHostService(PortFinder portFinder)
        {
            _portFinder = portFinder;
        }

        public async Task StartAsync()
        {
            var builder = WebApplication.CreateBuilder();
            
            // Find an available port
            Port = await _portFinder.FindAvailablePortAsync();
            
            // Configure the URL
            builder.WebHost.UseUrls($"http://localhost:{Port}");
            
            // 添加控制器
            builder.Services.AddControllers();
            
            _app = builder.Build();
            
            // 配置中间件
            _app.UseRouting();
            _app.MapControllers();

            await _app.StartAsync();
        }

        public async Task StopAsync()
        {
            if (_app != null)
            {
                await _app.StopAsync();
                await _app.DisposeAsync();
            }
        }
    }
}
