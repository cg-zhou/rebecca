using Microsoft.AspNetCore.Mvc;

namespace Rebecca.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FolderController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAsync()
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
}