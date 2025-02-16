using Rebecca.Services.Api;

namespace Rebecca.Controllers;

public class FolderController : IController
{
    public async Task<object?> SelectAsync()
    {
        return await Task.Run(() =>
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
    }
}
