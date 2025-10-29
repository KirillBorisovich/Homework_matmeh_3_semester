// <copyright file="Server.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace SimpleFTP;

using System.Net;
using System.Net.Sockets;

/// <summary>
/// A server that allows you to download files.
/// </summary>
/// <param name="port">The port on which the server will accept connections.</param>
public class Server(int port)
{
    private readonly int port = port;
    private volatile bool isStop;

    /// <summary>
    /// Start the server.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task Start()
    {
        using var listener = new TcpListener(IPAddress.Any, this.port);
        listener.Start();
        while (!this.isStop)
        {
            var socket = await listener.AcceptSocketAsync();
            _ = Task.Run(async () =>
            {
                using (socket)
                {
                    await using var stream = new NetworkStream(socket);
                    using var reader = new StreamReader(stream);
                    await using var writer = new StreamWriter(stream);
                    writer.AutoFlush = true;
                    while (!this.isStop)
                    {
                        try
                        {
                            await ProcessTheRequest(stream, reader, writer);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                        catch (IOException)
                        {
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            await writer.WriteAsync("-4 Server error\n");
                        }
                    }
                }
            });
        }
    }

    /// <summary>
    /// Stop the server.
    /// </summary>
    public void Stop()
    {
        this.isStop = true;
    }

    private static async Task ProcessTheRequest(Stream stream, StreamReader reader, StreamWriter writer)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        var data = await reader.ReadLineAsync(cts.Token);

        if (string.IsNullOrEmpty(data))
        {
            return;
        }

        switch (data[0])
        {
            case '1':
                await List(writer, data);
                return;
            case '2':
                await Get(stream, writer, data);
                return;
            default:
                await writer.WriteAsync("-2 Unknown request\n");
                return;
        }
    }

    private static async Task List(StreamWriter writer, string data)
    {
        string path;
        try
        {
            path = GetAPath(data);
        }
        catch (InvalidOperationException ex)
        {
            await writer.WriteAsync($"-3 {ex.Message}\n");
            return;
        }

        if (!Directory.Exists(path))
        {
            await writer.WriteAsync("-1\n");
            return;
        }

        var directories = Directory.GetDirectories(path);
        var files = Directory.GetFiles(path);

        var responseString = (directories.Length + files.Length).ToString();
        foreach (var directory in directories)
        {
            responseString += $" {directory[Directory.GetCurrentDirectory().Length..]} true";
        }

        foreach (var file in files)
        {
            responseString += $" {file[Directory.GetCurrentDirectory().Length..]} false";
        }

        responseString += "\n";
        await writer.WriteAsync(responseString);
    }

    private static async Task Get(Stream stream, StreamWriter writer, string data)
    {
        string path;
        try
        {
            path = GetAPath(data);
        }
        catch (InvalidOperationException ex)
        {
            await writer.WriteAsync($"-3 {ex.Message}\n");
            return;
        }

        if (!File.Exists(path))
        {
            await writer.WriteAsync("-1 File not found.\n");
            return;
        }

        if (Directory.Exists(path))
        {
            await writer.WriteAsync("-1 This is the folder.\n");
            return;
        }

        var fileInfo = new FileInfo(path);
        await writer.WriteAsync($"{fileInfo.Length}\n");

        await using var fileStream = File.OpenRead(path);
        const int sizeBuffer = 8192;
        var buffer = new byte[sizeBuffer];
        var bytesRead = 0;

        while ((bytesRead = await fileStream.ReadAsync(buffer)) > 0)
        {
            await stream.WriteAsync(buffer.AsMemory(0, bytesRead));
        }

        await stream.FlushAsync();
    }

    private static string GetAPath(string data)
    {
        var response = data.Split();
        string path;

        if (response.Length < 2 || response[1].Length <= 2)
        {
            path = Directory.GetCurrentDirectory();
        }
        else if (response[1].Contains(".."))
        {
            throw new InvalidOperationException("Using relative paths backwards is prohibited.");
        }
        else
        {
            path = Path.GetFullPath(Path.Combine(
                Directory.GetCurrentDirectory(),
                response[1][0] == '/' ? response[1][1..] : response[1]));
        }

        return path;
    }
}