using System.IO;

namespace Rebecca.Services;

public class LogService
{
    private readonly string _logPath;
    private static LogService? _instance;

    public static LogService Instance => _instance ??= new LogService();

    private LogService()
    {
        _logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Rebecca",
            "logs.txt"
        );
        Directory.CreateDirectory(Path.GetDirectoryName(_logPath)!);
    }

    public void Log(string message)
    {
        lock (this)
        {
            var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
            File.AppendAllText(_logPath, logMessage);
            System.Diagnostics.Debug.WriteLine(logMessage);
        }
    }
}
