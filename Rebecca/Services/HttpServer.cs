using Rebecca.Services.Api;
using System.Net;
using System.Reflection;

namespace Rebecca.Services;

public class HttpServer : IDisposable
{
    private readonly HttpListener _listener;
    private readonly Assembly _resourceAssembly;
    private readonly ControllerRegistry _apiHandlers;
    private bool _isRunning;

    public HttpServer(int port, Assembly resourceAssembly)
    {
        _resourceAssembly = resourceAssembly;
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://localhost:{port}/");
        _apiHandlers = new ControllerRegistry();
    }

    public void Start()
    {
        _listener.Start();
        _isRunning = true;
        Task.Run(HandleRequests);
    }

    public void Stop()
    {
        _isRunning = false;
        _listener.Stop();
    }

    private async Task HandleRequests()
    {
        while (_isRunning)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                await HandleRequestAsync(context);
            }
            catch (Exception ex) when (_listener.IsListening)
            {
                System.Diagnostics.Debug.WriteLine($"处理请求出错: {ex.Message}");
            }
        }
    }

    private async Task HandleRequestAsync(HttpListenerContext context)
    {
        try
        {
            var path = (context.Request.Url?.AbsolutePath ?? "").Trim('/');
            if (path.StartsWith("api/") || path.Equals("api"))
            {
                await _apiHandlers.HandleRequestAsync(context);
                return;
            }

            await HandleStaticFileAsync(context);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error processing request: {ex}");
            context.Response.StatusCode = 500;
        }
        finally
        {
            context.Response.Close();
        }
    }

    private async Task HandleStaticFileAsync(HttpListenerContext context)
    {
        try
        {
            var path = context.Request.Url?.LocalPath ?? "/";
            var indexHtmlPath = "/index.html";
            if (path == "/")
            {
                path = indexHtmlPath;
            };

            var stream = ResourceHelper.GetEmbeddedResource(path, _resourceAssembly);
            if (stream != null)
            {
                context.Response.ContentType = ResourceHelper.GetContentType(path);
            }
            else
            {
                stream = ResourceHelper.GetEmbeddedResource(indexHtmlPath, _resourceAssembly);
                context.Response.ContentType = "text/html";
            }

            using (stream)
            {
                await stream!.CopyToAsync(context.Response.OutputStream);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error processing request: {ex}");
            context.Response.StatusCode = 500;
        }
    }

    public void Dispose()
    {
        Stop();
        _listener.Close();
    }
}
