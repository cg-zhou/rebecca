namespace Rebecca.Services.Api;

[AttributeUsage(AttributeTargets.Method)]
public class HttpMethodAttribute : Attribute
{
    public string Method { get; }

    protected HttpMethodAttribute(string method)
    {
        Method = method;
    }
}

public class HttpGetAttribute : HttpMethodAttribute
{
    public HttpGetAttribute() : base("GET") { }
}

public class HttpPostAttribute : HttpMethodAttribute
{
    public HttpPostAttribute() : base("POST") { }
}

public class HttpPutAttribute : HttpMethodAttribute
{
    public HttpPutAttribute() : base("PUT") { }
}

public class HttpDeleteAttribute : HttpMethodAttribute
{
    public HttpDeleteAttribute() : base("DELETE") { }
}
