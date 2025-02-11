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
        var resourceName = @"wwwroot\" + path.Replace('/', '\\').TrimStart('\\');

        var names = assembly.GetManifestResourceNames();
        var fullResourceName = names.FirstOrDefault(x =>
            x.ToUpper() == resourceName.ToUpper());

        if (fullResourceName == null)
        {
            return null;
        }

        return assembly.GetManifestResourceStream(fullResourceName);
    }
}
