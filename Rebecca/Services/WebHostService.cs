using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rebecca.Core.WebSockets;
using Rebecca.Extensions;
using StdEx.IO;
using StdEx.Media.Tmdb;
using StdEx.Net;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace Rebecca.Services;

public class WebHostService
{
    private WebApplication? _app;
    private readonly ILogger<WebHostService> _logger;
    public int Port { get; private set; } = 0;

    // 添加只读属性判断是否调试模式
    private static bool IsDebugMode => Debugger.IsAttached;

    public WebHostService(ILogger<WebHostService> logger)
    {
        _logger = logger;
    }

    private async Task ServeResource(HttpContext context, string path, Assembly assembly)
    {
        var resourcePath = @$"wwwroot\{path}".Replace('/', '\\');
        _logger.LogInformation($"Looking for resource: {resourcePath}");

        using var stream = Assembly.GetExecutingAssembly().GetEmbeddedResource(resourcePath);
        if (stream == null)
        {
            context.Response.StatusCode = 404;
            return;
        }

        var contentType = ContentTypeUtils.GetContentType(resourcePath); ;
        context.Response.ContentType = contentType;

        _logger.LogInformation($"Serving resource: {resourcePath}");
        await stream.CopyToAsync(context.Response.Body);
    }

    public async Task StartAsync()
    {
        try
        {
            var builder = WebApplication.CreateBuilder();
            Port = PortUtils.FindAvailable(8080);
            _logger.LogInformation($"Selected port: {Port}");

            builder.WebHost.UseUrls($"http://localhost:{Port}");

            // 配置控制器和日志服务
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                });

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    policy =>
                    {
                        policy.SetIsOriginAllowed(_ => true) // 允许所有来源，包括 WebSocket
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials(); // 这对 WebSocket 连接是必需的
                    });
            });

            // 注册应用服务
            builder.Services.AddLogging(configure =>
            {
                configure.AddConsole();
                configure.AddDebug();
            });
            
            builder.Services.AddSingleton<WebSocketHub>();
            builder.Services.AddSingleton<WebSocketService>();
            
            // 注册基础服务
            builder.Services.AddSingleton<ITmdbSettingsService, TmdbSettingsService>();
            builder.Services.AddSingleton<MediaLibraryConfigService>();
            
            // 使用扩展方法注册所有媒体库相关服务
            builder.Services.AddMediaLibraryServices();

            _app = builder.Build();

            _app.UseRouting();
            _app.UseCors();

            // 添加WebSocket支持
            var webSocketOptions = new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromMinutes(2)
            };
            _app.UseWebSockets(webSocketOptions);

            // 映射 WebSocket 路由
            _app.Map("/ws", async context =>
            {
                var connector = context.RequestServices.GetRequiredService<WebSocketService>();
                await connector.HandleWebSocket(context);
            });

            // 添加详细的错误处理中间件
            _app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/json";

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        _logger.LogError($"发生错误: {contextFeature.Error}");

                        var response = new
                        {
                            StatusCode = context.Response.StatusCode,
                            Message = IsDebugMode ? contextFeature.Error.Message : "内部服务器错误",
                            Details = IsDebugMode ? contextFeature.Error.StackTrace : null
                        };

                        await context.Response.WriteAsJsonAsync(response);
                    }
                });
            });

            if (IsDebugMode)
            {
                _app.UseProxyToDevServer();
                _logger.LogInformation("Running in DEBUG mode, using proxy to dev server");
            }
            else
            {
                _logger.LogInformation("Running in RELEASE mode, using embedded resources");

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

                        _logger.LogInformation($"Request: {context.Request.Method} {context.Request.Path}");
                        await ServeResource(context, path, Assembly.GetExecutingAssembly());
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error serving resource: {ex.Message}");
                        await next();
                    }
                });

                // 列出所有嵌入的资源
                var resources = Assembly.GetExecutingAssembly().GetManifestResourceNames();
                _logger.LogInformation("Embedded resources:");
                foreach (var resource in resources)
                {
                    _logger.LogInformation($"  {resource}");
                }
            }

            _app.MapControllers();

            await _app.StartAsync();
            _logger.LogInformation("Web application started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error starting web application: {ex}");
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
