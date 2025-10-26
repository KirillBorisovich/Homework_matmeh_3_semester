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
    private bool isStop;

    /// <summary>
    /// Start the server.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task Start()
    {
        var listener = new TcpListener(IPAddress.Any, this.port);
        listener.Start();
        while (!this.isStop)
        {
            var socket = await listener.AcceptSocketAsync();
            await Task.Run(async () =>
            {
                var stream = new NetworkStream(socket);
                var reader = new StreamReader(stream);
                var writer = new StreamWriter(stream) { AutoFlush = true };
                while (socket.Connected && !this.isStop)
                {
                    await ProcessTheRequest(stream, reader,  writer);
                }

                /*Нужно ли закрывать?*/
                socket.Close();
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

    private static string GetAPath(string data)
    {
        var response = data.Split();
        string path;
        if (response.Length < 2 || response[1].Length < 2)
        {
            path = Directory.GetCurrentDirectory();
        }
        else
        {
            if (response[1].Length >= 5 && response[1][..5] == "./../")
            {
                throw new InvalidOperationException("Using relative paths backwards is prohibited.");
            }

            path = Path.GetFullPath(Path.Combine(
                Directory.GetCurrentDirectory(),
                response[1][0] == '/' ? response[1][1..] : response[1]));
        }

        return path;
    }

    private static async Task ProcessTheRequest(Stream stream, StreamReader reader, StreamWriter writer)
    {
        var data = await reader.ReadLineAsync();

        if (data == null)
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
                await writer.WriteAsync("-1 Unknown request\n");
                return;
        }
    }

    private static async Task List(StreamWriter writer, string data)
    {
        var path = GetAPath(data);

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
            responseString += $" {path}/{directory} true";
        }

        foreach (var file in files)
        {
            responseString += $" {path}/{file} false";
        }

        responseString += "\n";
        await writer.WriteAsync(responseString);
    }

    private static async Task Get(Stream stream, StreamWriter writer, string data)
    {
        var path = GetAPath(data);

        if (!File.Exists(path))
        {
            await writer.WriteAsync("-1 File not found.\n");
        }

        if (Directory.Exists(path))
        {
            await writer.WriteAsync("-1 This is the folder.\n");
        }

        var fileInfo = new FileInfo(path);
        await writer.WriteAsync($"{fileInfo.Name}\n");

        await using var fileStream = File.OpenRead(path);
        var buffer = new byte[8192];
        int bytesRead;

        while ((bytesRead = await fileStream.ReadAsync(buffer)) > 0)
        {
            await stream.WriteAsync(buffer.AsMemory(0, bytesRead));
        }

        await stream.FlushAsync();
    }
}