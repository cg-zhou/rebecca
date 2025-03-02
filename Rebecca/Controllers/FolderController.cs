using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace Rebecca.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FolderController : ControllerBase
{
    [HttpGet]
    [Route("select")]
    public async Task<IActionResult> SelectAsync()
    {
        var path = await Task.Run(() =>
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

            return new
            {
                success = !string.IsNullOrWhiteSpace(selectedPath),
                path = selectedPath
            };
        });
        return Ok(path);
    }

    [HttpPost]
    [Route("open")]
    public IActionResult Open([FromBody] OpenFolderRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Path))
            {
                return BadRequest("路径不能为空");
            }

            if (!Directory.Exists(request.Path))
            {
                return NotFound("路径不存在");
            }

            // 使用资源管理器打开文件夹
            System.Diagnostics.Process.Start("explorer.exe", request.Path);
            
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}

public class OpenFolderRequest
{
    public string Path { get; set; } = string.Empty;
}