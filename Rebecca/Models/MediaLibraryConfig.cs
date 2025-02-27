using System.Collections.Generic;

namespace Rebecca.Models
{
    public class MediaLibraryConfig
    {
        public List<string> LibraryPaths { get; set; } = new List<string>();
        public bool AutoScan { get; set; } = false;
        public int ScanIntervalMinutes { get; set; } = 60;
        public bool EnableNotifications { get; set; } = true;
    }
}