using System.Net;
using System.Reflection;
using Rebecca.Services.Api;

namespace Rebecca.Services;

public class HttpServer : IDisposable
{
    private readonly HttpListener _listener;
    private readonly Assembly _resourceAssembly;
    private readonly ApiHandlerRegistry _apiHandlers;
    private bool _isRunning;

    public HttpServer(int port, Assembly resourceAssembly)
    {
        _resourceAssembly = resourceAssembly;
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://localhost:{port}/");
        _apiHandlers = new ApiHandlerRegistry();
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
                ProcessRequest(context);
            }
            catch (Exception ex) when (_listener.IsListening)
            {
                System.Diagnostics.Debug.WriteLine($"处理请求出错: {ex.Message}");
            }
        }
    }

    private void ProcessRequest(HttpListenerContext context)
    {
        try
        {
            var path = context.Request.Url?.LocalPath ?? "/";
            if (path.StartsWith("/api/"))
            {
                HandleApiRequest(context);
                return;
            }

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
                stream!.CopyTo(context.Response.OutputStream);
            }
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

    private async void HandleApiRequest(HttpListenerContext context)
    {
        var path = context.Request.Url?.LocalPath ?? "/";
        var method = context.Request.HttpMethod;

        var handler = _apiHandlers.GetHandler(path);
        if (handler != null && handler.Method == method)
        {
            await handler.HandleAsync(context);
        }
        else
        {
            context.Response.StatusCode = 404;
        }
        context.Response.Close();
    }

    public void Dispose()
    {
        Stop();
        _listener.Close();
    }
}
