// <copyright file="Client.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace SimpleFTP;

using System.Net.Sockets;

/// <summary>
/// Client to server for downloading files.
/// </summary>
public class Client : IDisposable
{
    private readonly TcpClient client;
    private readonly Stream stream;
    private readonly StreamWriter writer;
    private readonly StreamReader reader;
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="Client"/> class.
    /// </summary>
    /// <param name="ip">The IP for the connection.</param>
    /// <param name="port">Connection port.</param>
    public Client(string ip, int port)
    {
        this.client = new TcpClient(ip, port);
        this.stream = this.client.GetStream();
        this.writer = new StreamWriter(this.stream) { AutoFlush = true };
        this.reader = new StreamReader(this.stream);
    }

    /// <summary>
    /// Request a list of files and folders.
    /// </summary>
    /// <param name="path">The path to the folder on the server.</param>
    /// <returns>Response to the request: the number, names, and flags that
    /// take the value "true" for directories and "false" for files.</returns>
    public async Task<string> List(string path)
    {
        ObjectDisposedException.ThrowIf(this.disposed, this);

        await this.writer.WriteLineAsync($"1 {path}\n");
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        var data = await this.reader.ReadLineAsync(cts.Token);
        ServerExceptionHandler(data);
        return data!;
    }

    /// <summary>
    /// Request file download.
    /// </summary>
    /// <param name="pathForServer">The path to the file on the server.</param>
    /// <param name="downloadPath">The path to the folder where to download the file.</param>
    /// <exception cref="PathFormatException">The path was entered incorrectly.</exception>
    /// <exception cref="InvalidDataException">Invalid file size response from server.</exception>
    /// <exception cref="IOException">Unexpected end of stream.</exception>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task Get(string pathForServer, string downloadPath)
    {
        if (pathForServer.Contains('\\'))
        {
            throw new PathFormatException("The path should start with '/'");
        }

        var fileName = Path.GetFileName(pathForServer);

        if (downloadPath.Length < 3)
        {
            downloadPath = Directory.GetCurrentDirectory();
        }

        ObjectDisposedException.ThrowIf(this.disposed, this);

        await this.writer.WriteLineAsync($"2 {pathForServer}\n");
        using var cts0 = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        var length = await this.reader.ReadLineAsync(cts0.Token);

        ServerExceptionHandler(length);

        if (!long.TryParse(length, out var fileSize))
        {
            throw new InvalidDataException("Invalid file size response from server");
        }

        downloadPath += downloadPath[^1] == '/' ? fileName : '/' + fileName;
        FileStream fileStream;
        try
        {
            fileStream = File.Create(downloadPath);
        }
        catch (Exception)
        {
            throw new PathFormatException("Something is wrong in the path for the download file.");
        }

        const int sizeBuffer = 8192;
        var buffer = new byte[sizeBuffer];
        var totalRead = 0;

        while (totalRead < fileSize)
        {
            var bytesToRead = (int)Math.Min(buffer.Length, fileSize - totalRead);
            using var cts1 = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            var bytesRead = await this.stream.ReadAsync(buffer, 0, bytesToRead, cts1.Token);

            if (bytesRead == 0)
            {
                throw new IOException("Unexpected end of stream");
            }

            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
            totalRead += bytesRead;
        }

        await fileStream.DisposeAsync();
    }

    /// <summary>
    /// Dispose of the object.
    /// </summary>
    public void Dispose()
    {
        this.writer.Dispose();
        this.reader.Dispose();
        this.stream.Dispose();
        this.client.Dispose();
        this.disposed = true;
    }

    private static void ServerExceptionHandler(string? data)
    {
        if (string.IsNullOrEmpty(data))
        {
            throw new IOException("Server closed connection");
        }

        switch (data[..2])
        {
            case "-1":
                if (data.Length > 3)
                {
                    throw new FileNotFoundException($"{data[3..]}");
                }

                throw new FileNotFoundException();
            case "-2":
                throw new PathFormatException($"{data[3..]}");
            case "-3":
                throw new InvalidOperationException($"{data[3..]}");
        }
    }
}