using System.IO;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

namespace UnityPacker
{
    public static class Unpacker
    {
        public static void Unpack(string inputFile, string outputFolder)
        {
            string fileName = Path.GetRandomFileName();
            string tempPath = Path.Combine(Path.GetTempPath(), fileName);
            Directory.CreateDirectory(tempPath);

            Decompress(inputFile, tempPath);

            string[] dirEntries = Directory.GetDirectories(tempPath);

            foreach (string dirEntry in dirEntries)
            {
                if (!File.Exists(Path.Combine(dirEntry, "pathname")) || !File.Exists(Path.Combine(dirEntry, "asset.meta")))
                {
                    // Invalid format
                    continue;
                }

                string targetPath = Path.Combine(outputFolder, File.ReadAllText(Path.Combine(dirEntry, "pathname")));
                string targetFolder = Path.GetDirectoryName(targetPath);
                string targetMetaPath = targetPath + ".meta";

                if (!Directory.Exists(targetFolder))
                    Directory.CreateDirectory(targetFolder);

                if (File.Exists(targetPath))
                    File.Delete(targetPath);

                if (File.Exists(targetMetaPath))
                    File.Delete(targetMetaPath);

                if (File.Exists(Path.Combine(dirEntry, "asset")))
                {
                    File.Copy(Path.Combine(dirEntry, "asset"), targetPath);
                }
                else
                {
                    Directory.CreateDirectory(targetPath);
                }
                File.WriteAllText(targetMetaPath, File.ReadAllText(Path.Combine(dirEntry, "asset.meta")));
            }

            // Clean up
            Directory.Delete(tempPath, true);
        }

        private static void Decompress(string inputFile, string tempPath)
        {
            using var stream = new FileStream(inputFile, FileMode.Open);
            using var zipStream = new GZipInputStream(stream);
            using var archive = TarArchive.CreateInputTarArchive(zipStream);
            archive.ExtractContents(tempPath);
        }
    }
}
