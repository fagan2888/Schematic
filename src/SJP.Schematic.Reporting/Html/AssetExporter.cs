﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html
{
    internal class AssetExporter
    {
        public Task SaveAssetsAsync(string directory, bool overwrite = true, CancellationToken cancellationToken = default)
        {
            if (directory.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(directory));

            var dirInfo = new DirectoryInfo(directory);
            return SaveAssetsAsync(dirInfo, overwrite, cancellationToken);
        }

        public Task SaveAssetsAsync(DirectoryInfo directory, bool overwrite = true, CancellationToken cancellationToken = default)
        {
            if (directory == null)
                throw new ArgumentNullException(nameof(directory));

            return SaveAssetsAsyncCore(directory, overwrite, cancellationToken);
        }

        private static async Task SaveAssetsAsyncCore(DirectoryInfo directory, bool overwrite, CancellationToken cancellationToken)
        {
            if (!directory.Exists)
                directory.Create();

            var resourceFiles = _fileProvider.GetDirectoryContents("/");
            foreach (var resourceFile in resourceFiles)
            {
                var relativePath = FileNameToRelativePath(resourceFile.Name);
                var qualifiedPath = Path.Combine(directory.FullName, relativePath);
                var targetFile = new FileInfo(qualifiedPath);
                var isGzipped = targetFile.Extension == ".gz";
                if (isGzipped)
                {
                    var gzRemovedFileName = Path.GetFileNameWithoutExtension(targetFile.Name);
                    var dirName = Path.GetDirectoryName(targetFile.FullName);
                    var fullPath = Path.Combine(dirName, gzRemovedFileName);
                    targetFile = new FileInfo(fullPath);
                }

                if (targetFile.Exists && overwrite)
                    targetFile.Delete();

                if (!targetFile.Directory.Exists)
                    targetFile.Directory.Create();

                using var stream = targetFile.OpenWrite();
                using var resourceStream = resourceFile.CreateReadStream();
                if (isGzipped)
                {
                    using var gzipStream = new GZipStream(resourceStream, CompressionMode.Decompress);
                    await gzipStream.CopyToAsync(stream, cancellationToken).ConfigureAwait(false);
                    await stream.FlushAsync().ConfigureAwait(false);
                }
                else
                {
                    await resourceStream.CopyToAsync(stream, cancellationToken).ConfigureAwait(false);
                    await stream.FlushAsync().ConfigureAwait(false);
                }
            }
        }

        private static string FileNameToRelativePath(string fileName)
        {
            if (fileName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(fileName));

            var pieces = fileName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            // not a file path, only a file name
            if (pieces.Length < 3)
                return fileName;

            var dirNamePieces = pieces.Take(pieces.Length - 2).ToArray();
            var dirName = Path.Combine(dirNamePieces);
            var fileNameWithExtension = pieces[^2] + "." + pieces[^1];

            // assuming no more than 2 extensions -- refactor if more are needed
            if (!_nonStandardExtensions.Contains("." + fileNameWithExtension))
                return Path.Combine(dirName, fileNameWithExtension);

            dirNamePieces = pieces.Take(pieces.Length - 3).ToArray();
            dirName = Path.Combine(dirNamePieces);
            fileNameWithExtension = pieces[^3] + "." + pieces[^2] + "." + pieces[^1];

            return Path.Combine(dirName, fileNameWithExtension);
        }

        private static readonly IEnumerable<string> _nonStandardExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".render.js", // for viz.js
            ".otf.woff",
            ".ttf.woff",
            ".otf.woff2",
            ".ttf.woff2",
            ".css.gz",
            ".js.gz"
        };

        private static readonly IFileProvider _fileProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly(), Assembly.GetExecutingAssembly().GetName().Name + ".assets");
    }
}
