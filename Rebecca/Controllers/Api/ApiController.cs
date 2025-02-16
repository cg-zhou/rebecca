using Rebecca.Services.Api;

namespace Rebecca.Controllers.Api;

public class ApiController : IController
{
    private readonly ControllerRegistry _registry;

    public ApiController(ControllerRegistry registry)
    {
        _registry = registry;
    }

    public Task<ApiInfo[]> GetAsync()
    {
        return Task.FromResult(_registry.GetRoutes()
            .Select(x => new ApiInfo(x.Path, x.HttpMethod))
            .ToArray());
    }
}
