using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using YamlDotNet.RepresentationModel;

namespace UnityPacker
{
    public static class Packer
    {
        public static void Pack(IDictionary<string, string> files, string outputFile)
        {
            string randomFile = Path.GetRandomFileName();

            string tempPath = Path.Combine(Path.GetTempPath(), randomFile);
            Directory.CreateDirectory(tempPath);
            AddAssets(files, tempPath);

            if (File.Exists(outputFile))
                File.Delete(outputFile);

            Compress(outputFile, tempPath);

            // Clean up
            Directory.Delete(tempPath, true);
        }

        private static void AddAssets(IDictionary<string, string> files, string tempPath)
        {
            foreach (KeyValuePair<string, string> fileEntry in files)
            {
                if (File.Exists(fileEntry.Key))
                    AddAsset(tempPath, fileEntry.Key, fileEntry.Value);
                else if (Directory.Exists(fileEntry.Key))
                    AddFolder(tempPath, fileEntry.Key, fileEntry.Value);
                else
                    throw new FileNotFoundException($"Could not find file or directory {fileEntry.Key}");
            }
        }

        private static void AddFolder(string tempPath, string folder, string destination)
        {
            string[] folders = Directory.GetDirectories(folder, "*", SearchOption.AllDirectories);
            string[] files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);

            var entries = new List<string>(folders);
            entries.AddRange(files);

            foreach (string filename in entries)
            {
                // metas will be copied with their asset
                if (Path.GetExtension(filename) == ".meta")
                    continue;

                string destinationPath = Path.Combine(destination, Path.GetRelativePath(folder, filename));

                // unitypackage is always using / for directory separator
                destinationPath = destinationPath.Replace(Path.DirectorySeparatorChar, '/');

                AddAsset(tempPath, filename, destinationPath);
            }
        }

        private static void AddAsset(string tempPath, string fromFile, string toPath)
        {
            YamlDocument meta = GetMeta(fromFile) ?? GenerateMeta(fromFile, toPath);

            string guid = GetGuid(meta);

            Directory.CreateDirectory(Path.Combine(tempPath, guid));

            if (File.Exists(fromFile))
            {
                string assetPath = Path.Combine(tempPath, guid, "asset");
                File.Copy(fromFile, assetPath);
            }

            string pathnamePath = Path.Combine(tempPath, guid, "pathname");
            File.WriteAllText(pathnamePath, toPath);

            string metaPath = Path.Combine(tempPath, guid, "asset.meta");
            SaveMeta(metaPath, meta);
        }

        private static void SaveMeta(string metaPath, YamlDocument meta)
        {
            using (var writer = new StreamWriter(metaPath))
            {
                new YamlStream(meta).Save(writer, false);
            }

            var metaFile = new FileInfo(metaPath);

            using FileStream metaFileStream = metaFile.Open(FileMode.Open);
            metaFileStream.SetLength(metaFile.Length - 3 - Environment.NewLine.Length);
        }

        private static string GetGuid(YamlDocument meta)
        {
            var mapping = (YamlMappingNode)meta.RootNode;

            var key = new YamlScalarNode("guid");

            var value = (YamlScalarNode)mapping[key];
            return value.Value;
        }

        private static YamlDocument GenerateMeta(string fromFile, string toFile)
        {
            string guid = Utils.CreateGuid(toFile);

            if (Directory.Exists(fromFile))
            {
                // this is a folder
                return new YamlDocument(new YamlMappingNode
                        {
                            {"guid", guid},
                            {"fileFormatVersion", "2"},
                            {"folderAsset", "yes"}
                        });
            }
            else
            {
                // this is a file
                return new YamlDocument(new YamlMappingNode
                        {
                            {"guid", guid},
                            {"fileFormatVersion", "2"}
                        });
            }
        }

        private static YamlDocument GetMeta(string filename)
        {
            // do we have a .meta file?
            string metaPath = filename + ".meta";

            if (!File.Exists(metaPath))
                return null;

            using var reader = new StreamReader(metaPath);
            var yaml = new YamlStream();
            yaml.Load(reader);

            return yaml.Documents[0];
        }

        private static void Compress(string outputFile, string tempPath)
        {
            using var stream = new FileStream(outputFile, FileMode.CreateNew);
            using var zipStream = new GZipOutputStream(stream);
            using var archive = TarArchive.CreateOutputTarArchive(zipStream);
            archive.RootPath = tempPath;
            archive.AddFilesRecursive(tempPath);
        }
    }
}
