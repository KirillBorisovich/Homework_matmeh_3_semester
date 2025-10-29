// <copyright file="Client.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace SimpleFTP;

using System.Net.Sockets;

public class Client : IDisposable
{
    private readonly TcpClient client;
    private readonly Stream stream;
    private readonly StreamWriter writer;
    private readonly StreamReader reader;
    private bool disposed;

    public Client(string ip, int port)
    {
        this.client = new TcpClient(ip, port);
        this.stream = this.client.GetStream();
        this.writer = new StreamWriter(this.stream) { AutoFlush = true };
        this.reader = new StreamReader(this.stream);
    }

    public async Task<string> List(string path)
    {
        ObjectDisposedException.ThrowIf(this.disposed, this);

        await this.writer.WriteLineAsync($"1 {path}\n");
        var data = await this.reader.ReadLineAsync();
        ServerExceptionHandler(data);
        return data!;
    }

    public async Task Get(string pathForServer, string downloadPath)
    {
        ObjectDisposedException.ThrowIf(this.disposed, this);

        await this.writer.WriteLineAsync($"2 {pathForServer}\n");
        var length = await this.reader.ReadLineAsync();

        ServerExceptionHandler(length);

        if (!long.TryParse(length, out var fileSize))
        {
            throw new InvalidDataException("Invalid file size response from server");
        }

        await using var fileStream = File.Create(downloadPath);
        var buffer = new byte[8192];
        var totalRead = 0;

        while (totalRead < fileSize)
        {
            var bytesToRead = (int)Math.Min(buffer.Length, fileSize - totalRead);
            var bytesRead = await this.stream.ReadAsync(buffer, 0, bytesToRead);

            if (bytesRead == 0)
            {
                throw new IOException("Unexpected end of stream");
            }

            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
            totalRead += bytesRead;
        }
    }

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