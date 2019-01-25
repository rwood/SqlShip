using System;
using System.IO;
using System.IO.Compression;
using SqlShip.Helpers;
using SqlShip.Interfaces;

namespace SqlShip.Services
{
    public class UpdatePackager : IUpdatePackager
    {
        private readonly IUpdatePackagerConfig _config;
        private readonly ILogger _logger;

        public UpdatePackager(IUpdatePackagerConfig config, ILogger logger)
        {
            _config = config;
            _logger = logger;
        }

        public bool Package()
        {
            var destFile =
                $"{_config.DestinationDirectory.FullName}\\{Path.GetFileNameWithoutExtension(_config.SourceDirectory.Name)}_{DateTime.Now.ToFileTimeUtc()}.zip";
            _logger.Information($"Writing: {destFile}");
            using (var zipToOpen = new FileStream(destFile, FileMode.Create))
            {
                using (var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
                {
                    WriteDirectoryToZip(_config.SourceDirectory, archive);
                }
            }

            return true;
        }

        private void WriteDirectoryToZip(DirectoryInfo sourceDir, ZipArchive archive)
        {
            WriteAllFilesInDirectoryToZip(sourceDir, archive);
            foreach (var directoryInfo in sourceDir.GetDirectories())
            {
                if (directoryInfo.Name.StartsWith(".")) continue;
                WriteDirectoryToZip(directoryInfo, archive);
            }
        }

        private void WriteAllFilesInDirectoryToZip(DirectoryInfo sourceDir, ZipArchive archive)
        {
            foreach (var file in sourceDir.GetFiles())
                WriteFileToZip(sourceDir, file, archive);
        }

        private void WriteFileToZip(DirectoryInfo source, FileInfo file, ZipArchive archive)
        {
            var fileName = file.Name.ToLower();
            if (fileName.StartsWith(".") ||
                fileName.Contains(".vshost") ||
                fileName.EndsWith(".delete") ||
                fileName.EndsWith(".config") ||
                fileName.EndsWith(".zip")) return;
            var readmeEntry = archive.CreateEntry(PathHelpers.GetRelativePath(source.FullName, file.FullName));
            _logger.Information($"{readmeEntry}");
            using (var writeStream = readmeEntry.Open())
            {
                var bufferSize = 1024;
                var buffer = new byte[bufferSize];
                int bytesRead;
                var stream = file.OpenRead();
                while ((bytesRead = stream.Read(buffer, 0, bufferSize)) > 0)
                    writeStream.Write(buffer, 0, bytesRead);
            }
        }
    }
}