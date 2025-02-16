using System.Net;
using System.Text;
using System.Text.Json;

namespace Rebecca.Services.Api;

public class SelectFolderHandler : IApiHandler
{
    public string Path => "/api/folder/select";
    public string Method => "GET";

    public async Task HandleAsync(HttpListenerContext context)
    {
        string? selectedPath = null;
        var thread = new Thread(() =>
        {
            using var folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                selectedPath = folderDialog.SelectedPath;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        var response = new { success = selectedPath != null, path = selectedPath };
        var jsonResponse = JsonSerializer.Serialize(response);
        var buffer = Encoding.UTF8.GetBytes(jsonResponse);

        context.Response.ContentType = "application/json";
        await context.Response.OutputStream.WriteAsync(buffer);
    }
}
