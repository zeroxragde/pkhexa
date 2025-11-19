using System;
using System.IO.Compression;

namespace PkHexA.Services
{
    public static class SpriteDataInitializerService
    {
        private const string ZipUrl =
            "https://drive.google.com/uc?export=download&id=1OvfLUvHzPmTC7k0EZgACSWtPfmyL8J_W";

        private static readonly string LocalSpritesPath =
            Path.Combine(FileSystem.AppDataDirectory, "pokehex");

        public static async Task EnsureSpriteDataAsync()
        {
            try
            {
                if (Directory.Exists(Path.Combine(LocalSpritesPath, "sprites")))
                    return;

                Directory.CreateDirectory(LocalSpritesPath);

                string tempZip = Path.Combine(FileSystem.CacheDirectory, "sprites.zip");
                await DownloadZipAsync(ZipUrl, tempZip);

                using var zip = ZipFile.OpenRead(tempZip);

                foreach (var entry in zip.Entries)
                {
                    if (string.IsNullOrWhiteSpace(entry.Name))
                        continue;

                    string fixedPath = entry.FullName.Replace("pokehex/", "");

                    string finalPath = Path.Combine(LocalSpritesPath, fixedPath);
                    Directory.CreateDirectory(Path.GetDirectoryName(finalPath)!);

                    entry.ExtractToFile(finalPath, overwrite: true);
                }

                File.Delete(tempZip);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SpriteDataInitializerService] ERROR: {ex}");
            }
        }

        private static async Task DownloadZipAsync(string url, string outputPath)
        {
            using var http = new HttpClient();
            var bytes = await http.GetByteArrayAsync(url);
            await File.WriteAllBytesAsync(outputPath, bytes);
        }

        public static string GetLocalSpritesRoot()
        {
            return LocalSpritesPath;
        }
    }
}
