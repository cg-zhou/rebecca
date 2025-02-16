using System.Reflection;

namespace Rebecca.Services.Api;

public class ActionInfo
{
    public ActionInfo(
        string path,
        string httpMethod,
        IController? instance,
        MethodInfo? handlerMethod)
    {
        Path = path;
        HttpMethod = httpMethod;
        Instance = instance;
        HandlerMethod = handlerMethod;
    }

    // API路由信息
    public string Path { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;

    // 运行时信息
    public IController? Instance { get; set; }
    public MethodInfo? HandlerMethod { get; set; }
}
