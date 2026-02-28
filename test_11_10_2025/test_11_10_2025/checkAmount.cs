// <copyright file="checkAmount.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace Test_11_10_2025;

using System.Security.Cryptography;
using System.Text;

public class CheckAmount
{
    private readonly MD5 md5 = MD5.Create();

    public async Task<byte[]?> CalculateTheCheckAmountSingleThreaded(string path)
    {
        switch (CheckPath(path))
        {
            case "file":
                return await this.CalculateTheHashOfAFileSingleThreaded(path);
            case "directory":
                return await this.CalculateTheHashOfADirectorySingleThreaded(path);
            case "none":
                throw new ArgumentException("Path cannot be null or empty", nameof(path));
            default:
                return null;
        }
    }

    private async Task<byte[]?> CalculateTheHashOfAFileSingleThreaded(string path)
    {
        var fileNameOnly = Path.GetFileName(path);
        var fileName = Encoding.UTF8.GetBytes(fileNameOnly);
        this.md5.ComputeHash(fileName);
        var fileInfo = new FileInfo(path);
        await using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        const int sizeBuffer = 8192;
        var buffer = new byte[sizeBuffer];
        var totalRead = 0;
        while (totalRead < fileInfo.Length)
        {
            var bytesToRead = (int)Math.Min(buffer.Length, fileInfo.Length - totalRead);
            var bytesRead = await fileStream.ReadAsync(buffer.AsMemory(0, bytesToRead));

            if (bytesRead == 0)
            {
                throw new IOException("Unexpected end of stream");
            }

            this.md5.ComputeHash(buffer);
            totalRead += bytesRead;
        }

        return this.md5.Hash;
    }

    private async Task<byte[]?> CalculateTheHashOfADirectorySingleThreaded(string path)
    {
        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException($"Directory not found: {path}");
        }

        var directoryInfo = new DirectoryInfo(path);

        var directoryNameBytes = Encoding.UTF8.GetBytes(directoryInfo.Name);
        this.md5.TransformBlock(directoryNameBytes, 0, directoryNameBytes.Length, null, 0);

        var items = directoryInfo.GetFileSystemInfos()
            .OrderBy(x => x.Name, StringComparer.Ordinal)
            .ToArray();

        foreach (var item in items)
        {
            switch (item)
            {
                case DirectoryInfo subDirectory:
                {
                    var subDirHash = await this.CalculateFileHashAsync(subDirectory.FullName);
                    if (subDirHash != null)
                    {
                        this.md5.TransformBlock(subDirHash, 0, subDirHash.Length, null, 0);
                    }

                    break;
                }

                case FileInfo file:
                {
                    var fileHash = await this.CalculateFileHashAsync(file.FullName);
                    if (fileHash != null)
                    {
                        this.md5.TransformBlock(fileHash, 0, fileHash.Length, null, 0);
                    }

                    break;
                }
            }
        }

        this.md5.TransformFinalBlock([], 0, 0);
        return this.md5.Hash;
    }

    private async Task<byte[]?> CalculateFileHashAsync(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, true);

        const int bufferSize = 8192;
        var buffer = new byte[bufferSize];
        var totalRead = 0;

        while (totalRead < fileInfo.Length)
        {
            var bytesToRead = (int)Math.Min(buffer.Length, fileInfo.Length - totalRead);
            var bytesRead = await fileStream.ReadAsync(buffer.AsMemory(0, bytesToRead));

            if (bytesRead == 0)
            {
                throw new IOException("Unexpected end of stream");
            }

            this.md5.TransformBlock(buffer, 0, bytesRead, null, 0);
            totalRead += bytesRead;
        }

        this.md5.TransformFinalBlock([], 0, 0);
        return this.md5.Hash;
    }

    private static string CheckPath(string path)
    {
        if (File.Exists(path))
        {
            return "file";
        }
        else if (Directory.Exists(path))
        {
            return "directory";
        }
        else
        {
            return "none";
        }
    }
}