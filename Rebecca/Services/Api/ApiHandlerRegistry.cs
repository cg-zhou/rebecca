using System.Reflection;

namespace Rebecca.Services.Api;

public class ApiHandlerRegistry
{
    private readonly Dictionary<string, IApiHandler> _handlers = new();

    public ApiHandlerRegistry()
    {
        RegisterHandlers();
    }

    private void RegisterHandlers()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var handlerTypes = assembly.GetTypes()
            .Where(t => !t.IsInterface && !t.IsAbstract && typeof(IApiHandler).IsAssignableFrom(t));

        foreach (var handlerType in handlerTypes)
        {
            var handler = (IApiHandler)Activator.CreateInstance(handlerType)!;
            _handlers[handler.Path] = handler;
        }
    }

    public IApiHandler? GetHandler(string path) => 
        _handlers.TryGetValue(path, out var handler) ? handler : null;
}
