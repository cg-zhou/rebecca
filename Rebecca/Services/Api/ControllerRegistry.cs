using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Rebecca.Services.Api;

public class ControllerRegistry
{
    private readonly Dictionary<string, ActionInfo> _controllers = new();

    public ControllerRegistry()
    {
        RegisterControllers();
    }

    private void RegisterControllers()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var controllerTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && t.IsAssignableTo(typeof(IController)));

        foreach (var controllerType in controllerTypes)
        {
            RegisterController(controllerType);
        }
    }

    private static readonly string[] ExcludedMethods = { "ToString", "GetHashCode", "Equals", "GetType" };

    private static IEnumerable<MethodInfo> GetControllerMethods(Type controllerType)
    {
        return controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => !ExcludedMethods.Contains(m.Name));
    }

    private void RegisterController(Type controllerType)
    {
        var controllerName = controllerType.Name.Replace("Controller", "").ToLower();

        // 检查构造函数参数
        var constructor = controllerType.GetConstructors().First();
        var parameters = constructor.GetParameters();
        var constructorArgs = parameters.Length > 0
            ? [this]
            : Array.Empty<object>();

        var controller = (IController)Activator.CreateInstance(controllerType, constructorArgs)!;

        foreach (var method in GetControllerMethods(controllerType))
        {
            var httpMethod = method.GetCustomAttribute<HttpMethodAttribute>()?.Method ?? "GET";
            var methodName = method.Name.Replace("Async", "").ToLower();

            var path = "api";
            if (controllerName != "api")
            {
                path += "/" + controllerName;
            }

            if (methodName != "get")
            {
                path += "/" + methodName;
            }

            _controllers[path] = new ActionInfo(path, httpMethod, controller, method);
        }
    }

    public ActionInfo[] GetRoutes()
    {
        return _controllers.Values.ToArray();
    }

    public async Task HandleRequestAsync(HttpListenerContext context)
    {
        var path = context.Request.Url?.AbsolutePath.Trim('/');
        if (path == null)
        {
            context.Response.StatusCode = 404;
            return;
        }

        if (!_controllers.TryGetValue(path, out var apiInfo)
            || apiInfo.HttpMethod != context.Request.HttpMethod)
        {
            context.Response.StatusCode = 404;
            return;
        }

        dynamic actionResult = apiInfo.HandlerMethod!.Invoke(apiInfo.Instance, Array.Empty<object>())!;
        var result = await actionResult;

        var jsonResponse = string.Empty;
        if (result != null)
        {
            jsonResponse = JsonSerializer.Serialize(result);
        }

        var buffer = Encoding.UTF8.GetBytes(jsonResponse);
        context.Response.ContentType = "application/json";
        await context.Response.OutputStream.WriteAsync(buffer);
    }
}
