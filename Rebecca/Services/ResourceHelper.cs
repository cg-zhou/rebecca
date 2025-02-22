using System.IO;
using System.Reflection;

namespace Rebecca.Services;

public class ResourceHelper
{
    public static string GetContentType(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext switch
        {
            ".html" => "text/html",
            ".js" => "application/javascript",
            ".css" => "text/css",
            ".json" => "application/json",
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".gif" => "image/gif",
            ".svg" => "image/svg+xml",
            ".ico" => "image/x-icon",
            _ => "application/octet-stream",
        };
    }

    public static Stream? GetEmbeddedResource(string path, Assembly assembly)
    {
        path = path.TrimStart('/');
        if (string.IsNullOrEmpty(path))
        {
            path = "index.html";
        }

        var resourcePath = $"Rebecca.wwwroot.{path.Replace('/', '.')}";
        var stream = assembly.GetManifestResourceStream(resourcePath);
        
        LogService.Instance.Log($"Trying to get resource: {resourcePath}, Found: {stream != null}");
        
        return stream;
    }

    public static string[] GetEmbeddedResourceNames(Assembly assembly)
    {
        return assembly.GetManifestResourceNames();
    }
}
