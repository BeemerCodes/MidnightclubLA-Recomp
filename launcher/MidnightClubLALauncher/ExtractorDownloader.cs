using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows;

namespace MidnightClubLA.Launcher
{
    public partial class ExtractorDownloader
    {
        public static async Task DownloadExtractXisoAsync(Action<string> onProgress)
        {
            string url = "https://github.com/XboxDev/extract-xiso/releases/download/build-202309010306/extract-xiso-windows-amd64.zip";
            string toolsDir = Path.Combine(PathHelper.GetRepositoryRoot(), "tools", "extract-xiso");
            string zipPath = Path.Combine(Path.GetTempPath(), "extract-xiso.zip");

            onProgress("Downloading extract-xiso...");
            using (var client = new WebClient())
            {
                await client.DownloadFileTaskAsync(url, zipPath);
            }

            onProgress("Extracting...");
            Directory.CreateDirectory(toolsDir);
            
            // Extract
            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.FullName.EndsWith("extract-xiso.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        entry.ExtractToFile(Path.Combine(toolsDir, "extract-xiso.exe"), true);
                    }
                }
            }
            
            File.Delete(zipPath);
            onProgress("Ready!");
        }
    }
}
