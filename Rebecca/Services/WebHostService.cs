using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using StdEx.IO;
using StdEx.Net;
using System.Diagnostics;
using System.Reflection;

namespace Rebecca.Services
{
    public class WebHostService
    {
        private WebApplication? _app;
        public int Port { get; private set; } = 0;

        // 添加只读属性判断是否调试模式
        private static bool IsDebugMode => Debugger.IsAttached;

        private async Task ServeResource(HttpContext context, string path, Assembly assembly)
        {
            var resourcePath = @$"wwwroot\{path}".Replace('/', '\\');
            LogService.Instance.Log($"Looking for resource: {resourcePath}");

            using var stream = Assembly.GetExecutingAssembly().GetEmbeddedResource(resourcePath);
            if (stream == null)
            {
                context.Response.StatusCode = 404;
                return;
            }

            var contentType = ContentTypeUtils.GetContentType(resourcePath); ;
            context.Response.ContentType = contentType;

            LogService.Instance.Log($"Serving resource: {resourcePath}");
            await stream.CopyToAsync(context.Response.Body);
        }

        public async Task StartAsync()
        {
            try
            {
                var builder = WebApplication.CreateBuilder();
                Port = PortUtils.FindAvailable(8080);
                LogService.Instance.Log($"Selected port: {Port}");

                builder.WebHost.UseUrls($"http://localhost:{Port}");
                builder.Services.AddControllers();

                _app = builder.Build();

                _app.UseRouting();

                if (IsDebugMode)
                {
                    _app.UseProxyToDevServer();
                    LogService.Instance.Log("Running in DEBUG mode, using proxy to dev server");
                }
                else
                {
                    LogService.Instance.Log("Running in RELEASE mode, using embedded resources");

                    _app.Use(async (context, next) =>
                    {
                        try
                        {
                            var path = context.Request.Path.Value?.TrimStart('/') ?? "";
                            if (string.IsNullOrEmpty(path))
                            {
                                path = "index.html";
                            }

                            if (context.Request.Path.ToString().StartsWith("/api"))
                            {
                                await next();
                                return;
                            }

                            LogService.Instance.Log($"Request: {context.Request.Method} {context.Request.Path}");
                            await ServeResource(context, path, Assembly.GetExecutingAssembly());
                        }
                        catch (Exception ex)
                        {
                            LogService.Instance.Log($"Error serving resource: {ex.Message}");
                            await next();
                        }
                    });

                    // 列出所有嵌入的资源
                    var resources = Assembly.GetExecutingAssembly().GetManifestResourceNames();
                    LogService.Instance.Log("Embedded resources:");
                    foreach (var resource in resources)
                    {
                        LogService.Instance.Log($"  {resource}");
                    }
                }

                _app.UseRouting();
                _app.MapControllers();

                await _app.StartAsync();
                LogService.Instance.Log("Web application started successfully");
            }
            catch (Exception ex)
            {
                LogService.Instance.Log($"Error starting web application: {ex}");
                throw;
            }
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
