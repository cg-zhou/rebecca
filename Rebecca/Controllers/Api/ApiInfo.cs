namespace Rebecca.Controllers.Api;

public class ApiInfo
{
    public ApiInfo(string path, string method)
    {
        Path = path;
        Method = method;
    }

    public string Path { get; set; }
    public string Method { get; set; }
}
