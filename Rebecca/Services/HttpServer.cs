using System.Net;
using System.Reflection;

namespace Rebecca.Services;

public class HttpServer : IDisposable
{
    private readonly HttpListener _listener;
    private readonly int _port;
    private readonly Assembly _resourceAssembly;
    private bool _isRunning;

    public HttpServer(int port, Assembly resourceAssembly)
    {
        _port = port;
        _resourceAssembly = resourceAssembly;
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://localhost:{port}/");
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
            if (path == "/")
            {
                path = "/index.html";
            };
            var stream = ResourceHelper.GetEmbeddedResource(path, _resourceAssembly);

            if (stream != null)
            {
                context.Response.ContentType = ResourceHelper.GetContentType(path);
                using (stream)
                {
                    stream.CopyTo(context.Response.OutputStream);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Resource not found: {path}");
                context.Response.StatusCode = 404;
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

    public void Dispose()
    {
        Stop();
        _listener.Close();
    }
}
