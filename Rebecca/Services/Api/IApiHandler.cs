using System.Net;

namespace Rebecca.Services.Api;

public interface IApiHandler
{
    string Path { get; }
    string Method { get; }
    Task HandleAsync(HttpListenerContext context);
}
